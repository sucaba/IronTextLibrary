using IronText.Algorithm;
using IronText.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace IronText.Runtime
{
    using IronText.Logging;
    using System.Diagnostics;
    using State = System.Int32;

    sealed class RnGlrParser<T> : IPushParser
    {
        private readonly RuntimeGrammar       grammar;
        private readonly int[]                conflictActionsTable;
        private readonly int[]                stateToPriorToken;
        private readonly TransitionDelegate   transition;
        private          IProducer<T>         producer;
        private Msg priorInput;

        private readonly Gss<T>               gss;
        private readonly Queue<PendingShift>  Q = new Queue<PendingShift>(4);
        private readonly IReductionQueue<T>   R;
        private readonly Dictionary<long,T>   N = new Dictionary<long,T>();
        private readonly int[]                tokenComplexity;
        private readonly RuntimeProduction[]  pendingReductions;
        private int                           pendingReductionsCount = 0;

        private bool                          accepted = false;
        private readonly ILogging             logging;
        private bool isVerifier;

        public RnGlrParser(
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

        private RnGlrParser(
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
                        this.R = new ReductionQueue<T>();
                        break;
                    }
                case ReductionOrder.ByRuleDependency:
                    {
                        this.R = new ReductionPathQueue<T>(tokenComplexity, grammar);
                        break;
                    }
            }
        }

        public IReceiver<Msg> Next(Msg envelope)
        {
            gss.BeginEdit();

            N.Clear();

            var front = gss.FrontArray;
            MsgData data = envelope.FirstData;
            do
            {
                int lookahead = data.Token;

                Actor(lookahead);
                Reducer(lookahead);

                if (accepted)
                {
                    int count = gss.Count;
                    for (int i = 0; i != count; ++i)
                    {
                        var node = front[i];
                        if (IsAccepting(node.State))
                        {
                            producer.Result = node.FirstLink.Label;
                            break;
                        }
                    }
                }

                var termValue = producer.CreateLeaf(envelope, data);
                N[GetNKey(lookahead, gss.CurrentLayer + 1)] = termValue;

                for (int i = 0; i != gss.Count; ++i)
                {
                    var frontNode = front[i];

                    // Plan shift
                    var shift = GetShift(frontNode.State, lookahead);
                    if (shift >= 0)
                    {
                        Q.Enqueue(new PendingShift(frontNode, shift, lookahead));
                    }
                }

                data = data.NextAlternative;
            }
            while (data != null);

            gss.PushLayer();
            Shifter();

            // Run reducer again to complete 
            // shift-reduce and goto-reduce actions.
            Reducer();

#if DIAGNOSTICS
            if (!isVerifier)
            {
                using (var graphView = new GvGraphView("GlrState" + gss.CurrentLayer + ".gv"))
                {
                    gss.WriteGraph(graphView, grammar, stateToPriorToken);
                }
            }
#endif

            if (gss.IsEmpty)
            {
                if (accepted)
                {
                    return FinalReceiver<Msg>.Instance;
                }

                if (isVerifier)
                {
                    return null;
                }

                gss.Undo(0); // restore state before the current input token

                {
                    var message = new StringBuilder();
                    message
                        .Append("Unexpected token ")
                        .Append(grammar.SymbolName(envelope.AmbToken))
                        .Append(" in state stacks: {");
                    bool firstStack = true;
                    for (int i = 0; i != gss.Count; ++i)
                    {
                        var node = gss.FrontArray[i];

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

                            n = n.FirstLink.LeftNode;
                        }

                        message.Append("]");
                    }

                    message.Append("}");

                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Verbose,
                            Location = envelope.Location,
                            Message = message.ToString()
                        });
                }

                return RecoverFromError(envelope);
            }

            gss.EndEdit();
            this.priorInput = envelope;
            return this;
        }

        public IReceiver<Msg> Done()
        {
            Loc location;

            if (!object.Equals(priorInput, default(Msg)))
            {
                location = priorInput.Location.GetEndLocation();
            }
            else
            {
                location = new Loc(1, 1, 1, 1);
            }

            var eoi = new Msg(PredefinedTokens.Eoi, null, null, location);
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
            for (int j = 0; j != gss.Count; ++j)
            {
                var fronNode = gss.FrontArray[j];
                QueuReductionPaths(fronNode, lookahead);
            }
        }

        private void Reducer(int lookahead = -1)
        {
            while (!R.IsEmpty)
            {
                GssReducePath<T> path = R.Dequeue();

                int X = path.Production.Outcome;
                int m = path.Size;

                GssNode<T> nodeOnTheLeft = path.LeftNode;
                T newValue = producer.CreateBranch(path.Production, (IStackLookback<T>)path);
                T value    = MergeReductionAlternatives(path, newValue);

                var goToAction = GetAction(nodeOnTheLeft.State, X);
                Debug.Assert(goToAction.Kind == ParserActionKind.Shift);

                State nextState = goToAction.State;
                GssNode<T> existingNextStateNode = gss.GetFrontNode(nextState, lookahead);

                // Goto on non-term produced by the production.
                var newLink = gss.Push(nodeOnTheLeft, nextState, value, lookahead);

                if (lookahead < 0)
                {
                    continue;
                }

                QueueReductionPathsAfterReduction(
                    existingNextStateNode ?? gss.GetFrontNode(nextState, lookahead),
                    newLink,
                    lookahead,
                    includeNonZeroPaths: m != 0,
                    includeZeroPaths: existingNextStateNode == null);
            }
        }

        private T MergeReductionAlternatives(GssReducePath<T> path, T newValue)
        {
            T result;

            var Nkey = GetNKey(path.Production.Outcome, path.LeftNode.Layer);

            T currentValue;
            if (N.TryGetValue(Nkey, out currentValue))
            {
                result = producer.Merge(currentValue, newValue, (IStackLookback<T>)path);
            }
            else
            {
                result = newValue;
            }

            N[Nkey] = result;
            return result;
        }

        private void QueueReductionPathsAfterReduction(
            GssNode<T> nextState,
            GssLink<T> newLink,
            int token,
            bool includeNonZeroPaths,
            bool includeZeroPaths)
        {
            GetReductions(nextState.State, token);
            if (includeZeroPaths)
            {
                for (int i = 0; i != pendingReductionsCount; ++i)
                {
                    var prod = pendingReductions[i];
                    if (prod.InputLength == 0)
                    {
                        R.Enqueue(nextState, prod);
                    }
                }
            }

            if (includeNonZeroPaths && newLink != null)
            {
                for (int i = 0; i != pendingReductionsCount; ++i)
                {
                    var prod = pendingReductions[i];
                    if (prod.InputLength != 0)
                    {
                        R.Enqueue(newLink, prod);
                    }
                }
            }
        }

        private void QueuReductionPaths(GssNode<T> frontNode, int token)
        {
            GetReductions(frontNode.State, token);

            for (int i = 0; i != pendingReductionsCount; ++i)
            {
                var prod = pendingReductions[i];
                if (prod.InputLength != 0)
                {
                    R.Enqueue(frontNode, prod);
                }
            }

            for (int i = 0; i != pendingReductionsCount; ++i)
            {
                var prod = pendingReductions[i];
                if (prod.InputLength == 0)
                {
                    R.Enqueue(frontNode, prod);
                }
            }
        }

        private void Shifter()
        {
            while (Q.Count != 0)
            {
                var shift = Q.Dequeue();
                var val = N[GetNKey(shift.Token, gss.CurrentLayer)];

                gss.Push(shift.FrontNode, shift.ToState, val);
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
            public PendingShift(GssNode<T> frontNode, State toState, int token)
            {
                this.FrontNode = frontNode;
                this.ToState = toState;
                this.Token = token;
            }

            public readonly GssNode<T> FrontNode;
            public readonly State ToState;
            public readonly int Token;
        }

        public IPushParser CloneVerifier()
        {
            var result = new RnGlrParser<T>(
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

        public IReceiver<Msg> ForceNext(params Msg[] input)
        {
            isVerifier = true;
            try
            {
                while (this.CloneVerifier().Feed(input) == null)
                {
                    if (gss.CurrentLayer == 0)
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

        private IReceiver<Msg> RecoverFromError(Msg currentInput)
        {
            if (currentInput.AmbToken == PredefinedTokens.Eoi)
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

            IReceiver<Msg> result = new LocalCorrectionErrorRecovery(grammar, this, logging);
            if (priorInput != null)
            {
                gss.Undo(1);
                result = result.Next(priorInput);
            }

            return result.Next(currentInput);
        }

        private static long GetNKey(long X, int c)
        {
            return (X << 32) + c;
        }

        private static int MakeFakeDestState(int state, int token)
        {
            return -((state << 16) + token);
        }        
    }
}
