using System;
using System.Linq;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Diagnostics;
using IronText.Extensibility;

namespace IronText.Framework
{
    using State = System.Int32;
    using Token = System.Int32;
    using System.Text;

    sealed class RnGlrParser<T> : IPushParser
    {
        private readonly BnfGrammar           grammar;
        private readonly int[]                conflictActionsTable;
        private readonly Token[]              stateToPriorToken;
        private readonly TransitionDelegate   transition;
        private          IProducer<T>         producer;
        private readonly ResourceAllocator    allocator;
        private Msg priorInput;

        private readonly Gss<T>               gss;
        private readonly Queue<PendingShift>  Q = new Queue<PendingShift>(4);
        private readonly IReductionQueue<T>   R;
        private readonly Dictionary<long,T> N = new Dictionary<long,T>();
        private readonly T[]                  nodeBuffer;
        private readonly int[]                tokenComplexity;

        private bool                          accepted = false;
        private readonly ILogging             logging;
        private bool isVerifier;

        public RnGlrParser(
            BnfGrammar          grammar,
            int[]               tokenComplexity,
            TransitionDelegate  transition,
            Token[]             stateToPriorToken,
            int[]               conflictActionsTable,
            IProducer<T>        producer,
            ResourceAllocator   allocator,
            ILogging            logging)
            : this(
                grammar,
                tokenComplexity,
                transition,
                stateToPriorToken,
                conflictActionsTable,
                producer,
                allocator,
                logging,
                new Gss<T>(stateToPriorToken.Length))
        {
        }

        private RnGlrParser(
            BnfGrammar          grammar,
            int[]               tokenComplexity,
            TransitionDelegate  transition,
            Token[]             stateToPriorToken,
            int[]               conflictActionsTable,
            IProducer<T>        producer,
            ResourceAllocator   allocator,
            ILogging            logging,
            Gss<T>              gss)
        {
            this.grammar              = grammar;
            this.tokenComplexity      = tokenComplexity;
            this.transition           = transition;
            this.stateToPriorToken    = stateToPriorToken;
            this.conflictActionsTable = conflictActionsTable;
            this.gss                  = gss;
            this.nodeBuffer           = new T[grammar.MaxRuleSize];
            this.producer             = producer;
            this.allocator            = allocator;
            this.logging              = logging;

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

        public IReceiver<Msg> Next(Msg item)
        {
            gss.BeginEdit();

            Actor(item);
            Reducer(item.Id);

            if (accepted)
            {
                foreach (var node in gss.Front)
                {
                    if (IsAccepting(node.State))
                    {
                        producer.Result = node.PrevLink.Label;
                        break;
                    }
                }
            }

            var itemValue = producer.CreateLeaf(item);
            
            for (int i = 0; i != gss.Front.Count; ++i)
            {
                var frontNode = gss.Front[i];

                // Plan shift
                var shift = GetShift(frontNode.State, item.Id);
                if (shift >= 0)
                {
                    Q.Enqueue(new PendingShift(frontNode, shift));
                }

                // Shift and plan reduce
                var action = GetShiftReduce(frontNode.State, item.Id);
                if (action.Kind == ParserActionKind.ShiftReduce)
                {
                    PlanShiftReduce(
                        frontNode,
                        item.Id,
                        itemValue,
                        action.Rule,
                        action.Size);
                }
            }

            gss.PushLayer();
            Shifter(item, itemValue);

            // Run reducer again to complete 
            // shift-reduce and goto-reduce actions.
            Reducer();

#if DIAGNOSTICS
            using (var graphView = new GvGraphView("GlrState" + gss.CurrentLayer + ".gv"))
            {
                gss.WriteGraph(graphView, grammar, stateToPriorToken);
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
                        .Append(grammar.TokenName(item.Id))
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

                            n = n.PrevLink.LeftNode;
                        }

                        message.Append("]");
                    }

                    message.Append("}");

                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Verbose,
                            Location = item.Location,
                            HLocation = item.HLocation,
                            Message = message.ToString()
                        });
                }

                return RecoverFromError(item);
            }

            gss.EndEdit();
            this.priorInput = item;
            return this;
        }

        public IReceiver<Msg> Done()
        {
            Loc location;
            HLoc hLocation;

            if (!object.Equals(priorInput, default(Msg)))
            {
                location = priorInput.Location.GetEndLocation();
                hLocation = priorInput.HLocation.GetEndLocation();
            }
            else
            {
                location = Loc.Unknown;
                hLocation = HLoc.Unknown;
            }

            var eoi = new Msg(BnfGrammar.Eoi, null, location, hLocation);
            return Next(eoi);
        }

        private bool IsAccepting(State s)
        {
            int cell = transition(s, BnfGrammar.Eoi);
            return ParserAction.GetKind(cell) == ParserActionKind.Accept;
        }

        private void Actor(Msg item)
        {
            foreach (var w in gss.Front)
            {
                foreach (var red in GetReductions(w.State, item.Id))
                {
                    R.Enqueue(w, red.Rule, red.Size);
                }
            }
        }

        private void Reducer(int lookaheadToken = -1)
        {
            N.Clear();

            while (!R.IsEmpty)
            {
                GssReducePath<T> path = R.Dequeue();

                Token X = path.Rule.Left;
                int m = path.Size;

                GssNode<T> u = path.LeftNode;
                State k = u.State;
                T z;
                if (m == 0)
                {
                    z = producer.GetEpsilonNonTerm(X, (IStackLookback<T>)u);
                }
                else
                {
                    path.CopyDataTo(nodeBuffer);
                    T Λ = producer.CreateBranch(
                            path.Rule,
                            new ArraySlice<T>(nodeBuffer, 0, path.Size),
                            lookback: path.LeftNode);

                    int c = u.Layer;
                    T currentValue;
                    var Nkey = GetNKey(X, c);
                    if (N.TryGetValue(Nkey, out currentValue))
                    {
                        z = producer.Merge(currentValue, Λ, (IStackLookback<T>)u);
                    }
                    else
                    {
                        z = Λ;
                    }

                    N[Nkey] = z;
                }

                State l;

                var action = ParserAction.Decode(transition(k, X));
                switch (action.Kind)
                {
                    case ParserActionKind.Shift:
                        l = action.State;
                        break;
                    case ParserActionKind.ShiftReduce:
                        // Handle goto-reduce action
                        PlanShiftReduce(
                            u,
                            X,
                            z,
                            action.Rule,
                            action.Size);
                        continue;
                    default:
                        throw new InvalidOperationException(
                            "Internal error: Non-term action should be shift or shift-reduce, but got "
                            + Enum.GetName(typeof(ParserActionKind), action.Kind));
                }

                bool stateAlreadyExists = gss.GetFrontNode(l) != null;

                // Goto on non-term produced by rule.
                var newLink = gss.Push(u, l, z);

                if (lookaheadToken < 0)
                {
                    continue;
                }

                if (stateAlreadyExists)
                {
                    if (newLink != null && m != 0)
                    {
                        var reductions = GetReductions(l, lookaheadToken);
                        foreach (var red in reductions)
                        {
                            if (red.Size != 0)
                            {
                                R.Enqueue(newLink, red.Rule, red.Size);
                            }
                        }
                    }
                }
                else
                {
                    var w = gss.GetFrontNode(l);

                    var reductions = GetReductions(l, lookaheadToken);
                    foreach (var red in reductions)
                    {
                        if (red.Size == 0)
                        {
                            R.Enqueue(w, red.Rule, 0);
                        }
                    }

                    if (m != 0)
                    {
                        foreach (var red in reductions)
                        {
                            if (red.Size != 0)
                            {
                                R.Enqueue(newLink, red.Rule, red.Size);
                            }
                        }
                    }
                }
            }
        }

        private void Shifter(Msg item, T val)
        {
            // TODO: Is following useful for terms? Shift-Shift conlicts?
            N[GetNKey(item.Id, gss.CurrentLayer)] = val;

            while (Q.Count != 0)
            {
                var shift = Q.Dequeue();

                gss.Push(shift.FrontNode, shift.ToState, val);
            }
        }

        private void PlanShiftReduce(GssNode<T> frontNode, int shiftToken, T shiftValue, int rule, int size)
        {
            int fakeState = MakeFakeDestState(frontNode.State, shiftToken);

            var newLink = gss.Push(frontNode, fakeState, shiftValue);
            if (newLink != null)
            {
                R.Enqueue(newLink, grammar.Rules[rule], size);
            }
        }

        
        private int GetShift(State state, Token token)
        {
            int shift = -1;

            ParserAction action = GetDfaCell(state, token);
            switch (action.Kind)
            {
                case ParserActionKind.Shift:
                    shift = action.State;
                    break;
                case ParserActionKind.Conflict:
                    foreach (ParserAction conflictAction in GetConflictActions(action.Value1, action.Value2))
                    {
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

        private ParserAction GetShiftReduce(State state, Token token)
        {
            ParserAction action = GetDfaCell(state, token);
            switch (action.Kind)
            {
                case ParserActionKind.ShiftReduce:
                    return action;
                case ParserActionKind.Conflict:
                    foreach (ParserAction conflictAction in GetConflictActions(action.Value1, action.Value2))
                    {
                        if (conflictAction.Kind == ParserActionKind.ShiftReduce)
                        {
                            return conflictAction;
                        }
                    }

                    break;
            }

            return ParserAction.FailAction;
        }

        private IEnumerable<ModifiedReduction> GetReductions(State state, Token token)
        {
            ParserAction action = GetDfaCell(state, token);
            BnfRule rule;
            switch (action.Kind)
            {
                case ParserActionKind.Reduce:
                    rule = grammar.Rules[action.Rule];
                    yield return new ModifiedReduction(rule, action.Size);
                    break;
                case ParserActionKind.Accept:
                    accepted = true;
                    break;
                case ParserActionKind.Conflict:
                    foreach (ParserAction conflictAction in GetConflictActions(action.Value1, action.Value2))
                    {
                        switch (conflictAction.Kind)
                        {
                            case ParserActionKind.Reduce:
                                var crule = grammar.Rules[conflictAction.Rule];
                                yield return new ModifiedReduction(crule, conflictAction.Size);
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

        private ParserAction GetDfaCell(State state, Token token)
        {
            return ParserAction.Decode(transition(state, token));
        }

        struct PendingShift
        {
            public PendingShift(GssNode<T> frontNode, State toState)
            {
                FrontNode = frontNode;
                ToState = toState;
            }

            public readonly GssNode<T> FrontNode;
            public readonly State ToState;
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
                            allocator,
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
            this.producer = producer.GetErrorRecoveryProducer();

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
