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
        private bool                          isVerifier;
        private T                             currentTermValue;

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
                ProcessTerm(message, data);
            }

            gss.PushLayer();

#if false
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

        private void ProcessTerm(Message message, MessageData data)
        {
            this.currentTermValue = producer.CreateLeaf(message, data);

            int token = data.Token;

            foreach (var fromNode in gss.Front)
            {
                OnNewNode(fromNode, token, message, data);
            }

            Reducer(token, message, data);

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

        private void Reducer(int lookahead, Message message, MessageData data)
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
                    var newNode = gss.GetFrontNode(toState, lookahead);

                    OnNewNode(newNode, lookahead, message, data);
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

        private void OnNewNode(GssNode<T> frontNode, int lookahead, Message message, MessageData data)
        {
            Debug.Assert(frontNode == gss.GetFrontNode(frontNode.State, lookahead));

            Process(frontNode, lookahead, newLinkOnly: false);
        }

        private void OnNewLink(GssNode<T> existingToNode, GssBackLink<T> newLink, int lookahead)
        {
            Process(existingToNode, lookahead, newLinkOnly: true);
        }

        private void Process(GssNode<T> node, int token, bool newLinkOnly)
        {
            int start = transition(node.State, token);
            Process(node, token, newLinkOnly, start);
        }

        private void Process(GssNode<T> node, int token, bool newLinkOnly, int start)
        {
            int pos = start;
            while (true)
            {
                var action = grammar.Instructions[pos];

                switch (action.Kind)
                {
                    case ParserActionKind.Accept:
                        accepted = true;
                        return;
                    case ParserActionKind.Fork:
                        Process(node, token, newLinkOnly, action.Value1);
                        break;
                    case ParserActionKind.Reduce:
                        var prod = grammar.Productions[action.ProductionId];
                        if (!newLinkOnly || prod.InputLength != 0)
                        {
                            R.Enqueue(node, prod);
                        }
                        break;
                    case ParserActionKind.Shift:
                        if (!newLinkOnly)
                        {
                            gss.PushShift(
                                node,
                                action.State,
                                currentTermValue);
                        }
                        break;
                    case ParserActionKind.Fail:
                    case ParserActionKind.Restart:
                    case ParserActionKind.Exit:
                        return;
                    default:
                        throw new NotSupportedException($"Instruction '{action.Kind}' is not supported by GLR parser.");
                }

                ++pos;
            }
        }

        private int GoTo(int fromState, int token)
        {
            var action = GetAction(fromState, token);
            Debug.Assert(action.Kind == ParserActionKind.Shift);

            return action.State;
        }

        private bool IsAccepting(State s)
        {
            var action = GetAction(s, PredefinedTokens.Eoi);
            return action.Kind == ParserActionKind.Accept;
        }

        private ParserAction GetAction(State state, int token)
        {
            int start = transition(state, token);
            return grammar.Instructions[start];
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
