using System.Collections.Generic;
using System.Text;
using System;
using System.Diagnostics;
using IronText.Logging;
using IronText.Collections;

namespace IronText.Runtime
{
    using Diagnostics;
    using State = System.Int32;

    sealed class GlrParser<T> : IPushParser
    {
        private readonly RuntimeGrammar       grammar;
        private readonly int[]                conflictActionsTable;
        private readonly int[]                stateToPriorToken;
        private readonly TransitionDelegate   transition;
        private          IProducer<T>         producer;
        private Message  priorInput;

        private readonly Gss<T>               gss;
        private readonly IReductionQueue<T>   R;
        private readonly int[]                tokenComplexity;
        private readonly RuntimeProduction[]  pendingReductions;
        private int                           pendingReductionsCount = 0;

        private bool                          accepted = false;
        private readonly ILogging             logging;
        private bool isVerifier;

        public GlrParser(
            RuntimeGrammar      grammar,
            int[]               tokenComplexity,
            TransitionDelegate  transition,
            int[]               stateToPriorToken,
            int[]               conflictActionsTable,
            IProducer<T>        producer,
            ILogging            logging)
            : this(
                grammar,
                tokenComplexity,
                transition,
                stateToPriorToken,
                conflictActionsTable,
                producer,
                logging,
                new Gss<T>(stateToPriorToken.Length + grammar.Productions.Length))
        {
        }

        private GlrParser(
            RuntimeGrammar      grammar,
            int[]               tokenComplexity,
            TransitionDelegate  transition,
            int[]               stateToPriorToken,
            int[]               conflictActionsTable,
            IProducer<T>        producer,
            ILogging            logging,
            Gss<T>              gss)
        {
            this.grammar              = grammar;
            this.tokenComplexity      = tokenComplexity;
            this.transition           = transition;
            this.stateToPriorToken    = stateToPriorToken;
            this.conflictActionsTable = conflictActionsTable;
            this.gss                  = gss;
            this.producer             = producer;
            this.logging              = logging;

            this.pendingReductions = new RuntimeProduction[grammar.Productions.Length];

            switch (producer.ReductionOrder)
            {
                case ReductionOrder.Unordered:
                    {
                        this.R = new ReductionPathQueue<T>();
                        break;
                    }
                case ReductionOrder.ByRuleDependency:
                    {
                        this.R = new ReductionPathPriorityQueue<T>(tokenComplexity, grammar);
                        break;
                    }
            }
        }

        public IReceiver<Message> Next(Message message)
        {
            gss.BeginEdit();

            foreach (var data in message.Data.Alternatives())
            {
                ProcessAsLookahead(message, data);
                ProcessAsShift(message, data);
            }

            gss.PushLayer();

#if DIAGNOSTICS
            if (!isVerifier)
            {
                using (var graphView = new GvGraphView("GlrState" + gss.CurrentLayer + ".gv"))
                {
                    gss.WriteGraph(graphView, grammar, stateToPriorToken);
                }
            }
#endif

            if (gss.Front.IsEmpty)
            {
                if (accepted)
                {
                    return FinalReceiver<Message>.Instance;
                }

                if (isVerifier)
                {
                    return null;
                }

                gss.Undo(0); // restore state before the current input token

                LogError(message);

                return RecoverFromError(message);
            }

            gss.EndEdit();
            this.priorInput = message;
            return this;
        }

        private void ProcessAsShift(Message message, MessageData termData)
        {
            var termValue = producer.CreateLeaf(message, termData);

            foreach (var priorNode in gss.Front)
            {
                if (priorNode.Lookahead < 0 || priorNode.Lookahead == termData.Token)
                {
                    var toState = GetShift(priorNode.State, termData.Token);
                    if (toState >= 0)
                    {
                        gss.PushShift(
                            priorNode,
                            toState,
                            termValue);
                    }
                }
            }
        }

        private void ProcessAsLookahead(Message message, MessageData data)
        {
            int lookahead = data.Token;

            Actor(lookahead);
            Reducer(lookahead);

            if (accepted)
            {
                foreach (var node in gss.Front)
                {
                    if (IsAccepting(node.State))
                    {
                        producer.Result = node.BackLink.Label;
                        break;
                    }
                }
            }
        }

        public IReceiver<Message> Done()
        {
            Loc location;

            if (!object.Equals(priorInput, default(Message)))
            {
                location = priorInput.Location.GetEndLocation();
            }
            else
            {
                location = new Loc(1, 1, 1, 1);
            }

            var eoi = new Message(PredefinedTokens.Eoi, null, null, location);
            return Next(eoi);
        }

        private bool IsAccepting(State s)
        {
            var action = GetAction(s, PredefinedTokens.Eoi);
            return action.Kind == ParserActionKind.Accept;
        }

        private ParserAction GetAction(State state, int token)
        {
            int start = transition(state, token);
            if (start < 0)
            {
                return ParserAction.FailAction;
            }

            var result = grammar.Instructions[start];
            return result;
        }

        private void Actor(int lookahead)
        {
            foreach (var fromNode in gss.Front)
            {
                OnNewNode(fromNode, lookahead);
            }
        }

        private void Reducer(int lookahead)
        {
            while (!R.IsEmpty)
            {
                GssReducePath<T> path = R.Dequeue();

                var fromNode = path.LeftNode;

                int toState = GoTo(fromNode.State, path.Production.Outcome);
                var existingToNode = gss.GetFrontNode(toState, lookahead);

                T branch = producer.CreateBranch(path.Production, (IStackLookback<T>)path);

                var newLink = gss.PushReduced(
                                fromNode,
                                toState,
                                branch,
                                lookahead);

                if (existingToNode == null)
                {
                    OnNewNode(gss.GetFrontNode(toState, lookahead), lookahead);
                }
                else if (newLink != null)
                {
                    OnNewLink(existingToNode, newLink, lookahead);
                }
                else
                {
                    OnNewLabel(fromNode, existingToNode, branch, path);
                }
            }
        }

        private void OnNewLabel(
            GssNode<T> priorNode,
            GssNode<T> nextNode,
            T          newLabel,
            IStackLookback<T> lookback)
        {
            var link = nextNode.ResolveBackLink(priorNode);
            var value = producer.Merge(link.Label, newLabel, lookback);
            link.AssignLabel(value);
        }

        private void OnNewNode(GssNode<T> frontNode, int lookahead)
        {
            Debug.Assert(frontNode == gss.GetFrontNode(frontNode.State, lookahead));

            int toState = frontNode.State;
            GetReductions(toState, lookahead);

            for (int i = 0; i != pendingReductionsCount; ++i)
            {
                var prod = pendingReductions[i];
                R.Enqueue(frontNode, prod);
            }
        }

        private void OnNewLink(GssNode<T> existingToNode, GssBackLink<T> newLink, int lookahead)
        {
            GetReductions(existingToNode.State, lookahead);
            
            for (int i = 0; i != pendingReductionsCount; ++i)
            {
                var prod = pendingReductions[i];
                if (prod.InputLength != 0)
                {
                    if (newLink != null)
                    {
                        R.Enqueue(newLink, prod);
                    }
                    else
                    {
                        R.Enqueue(existingToNode, prod);
                    }

                }
            }
        }

        private int GetShift(State state, int token)
        {
            int shift = -1;

            ParserAction action = GetAction(state, token);
            switch (action.Kind)
            {
                case ParserActionKind.Shift:
                    shift = action.State;
                    break;
                case ParserActionKind.Conflict:
                    int start = action.Value1;
                    int last = action.Value1 + action.ConflictCount;
                    while (start != last)
                    {
                        var conflictAction = ParserAction.Decode(conflictActionsTable[start++]);
                        if (conflictAction.Kind == ParserActionKind.Shift)
                        {
                            shift = conflictAction.State;
                            break;
                        }
                    }
                    break;
            }

            return shift;
        }

        private int GoTo(int fromState, int token)
        {
            var goToAction = GetAction(fromState, token);
            Debug.Assert(goToAction.Kind == ParserActionKind.Shift);

            var toState = goToAction.State;
            return toState;
        }

        private void GetReductions(State state, int token)
        {
            pendingReductionsCount = 0;

            ParserAction action = GetAction(state, token);
            RuntimeProduction rule;
            switch (action.Kind)
            {
                case ParserActionKind.Reduce:
                    rule = grammar.Productions[action.ProductionId];
                    pendingReductionsCount = 1;
                    pendingReductions[0] = rule;
                    break;
                case ParserActionKind.Accept:
                    accepted = true;
                    break;
                case ParserActionKind.Conflict:
                    int start = action.Value1;
                    int last = action.Value1 + action.ConflictCount;
                    while (start != last)
                    {
                        var conflictAction = ParserAction.Decode(conflictActionsTable[start++]);
                        switch (conflictAction.Kind)
                        {
                            case ParserActionKind.Reduce:
                                var crule = grammar.Productions[conflictAction.ProductionId];
                                pendingReductions[pendingReductionsCount++] = crule;
                                break;
                        }
                    }

                    break;
            }
        }

        private IEnumerable<ParserAction> GetConflictActions(int start, short count)
        {
            int last = start + count;
            while (start != last)
            {
                yield return ParserAction.Decode(conflictActionsTable[start++]);
            }
        }

        struct PendingShift
        {
            public PendingShift(GssNode<T> frontNode, State toState, int token, T value)
            {
                this.FrontNode = frontNode;
                this.ToState = toState;
                this.Token = token;
                this.Value = value;
            }

            public readonly GssNode<T> FrontNode;
            public readonly State ToState;
            public readonly int Token;
            public readonly T Value;
        }

        public IPushParser CloneVerifier()
        {
            var result = new GlrParser<T>(
                            grammar,
                            tokenComplexity,
                            transition,
                            stateToPriorToken,
                            conflictActionsTable,
                            NullProducer<T>.Instance,
                            NullLogging.Instance,
                            gss.CloneWithoutData());

            result.isVerifier = true;

            return result;
        }

        public IReceiver<Message> ForceNext(params Message[] input)
        {
            isVerifier = true;
            try
            {
                while (!CanParse(input))
                {
                    if (!gss.HasLayers)
                    {
                        return null;
                    }

                    gss.PopLayer();
                }
            }
            finally
            {
                isVerifier = false;
            }

            this.Feed(input);

            return this;
        }

        private bool CanParse(Message[] input)
        {
            return this.CloneVerifier().Feed(input) != null;
        }

        private IReceiver<Message> RecoverFromError(Message currentInput)
        {
            if (currentInput.AmbiguousToken == PredefinedTokens.Eoi)
            {
                if (!isVerifier)
                {
                    logging.Write(
                        new LogEntry
                        {
                            Severity  = Severity.Error,
                            Message   = "Unexpected end of file.",
                            Location = currentInput.Location,
                        });
                }

                return null;
            }

            this.producer = producer.GetRecoveryProducer();

            IReceiver<Message> result = new LocalCorrectionErrorRecovery(grammar, this, logging);
            if (priorInput != null)
            {
                gss.Undo(1);
                result = result.Next(priorInput);
            }

            return result.Next(currentInput);
        }

        private void LogError(Message envelope)
        {
            var message = new StringBuilder();
            message
                .Append("Unexpected token ")
                .Append(grammar.SymbolName(envelope.AmbiguousToken))
                .Append(" in state stacks: {");
            bool firstStack = true;
            foreach (var node in gss.Front)
            {
                if (firstStack)
                {
                    firstStack = false;
                }
                else
                {
                    message.Append(", ");
                }

                message.Append("[");
                var n = node;
                bool firstState = true;
                while (true)
                {
                    if (firstState)
                    {
                        firstState = false;
                    }
                    else
                    {
                        message.Append(", ");
                    }

                    message.Append(n.State);
                    if (n.State == 0)
                    {
                        break;
                    }

                    n = n.BackLink.PriorNode;
                }

                message.Append("]");
            }

            message.Append("}");

            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Verbose,
                    Location = envelope.Location,
                    Message  = message.ToString()
                });
        }
    }
}
