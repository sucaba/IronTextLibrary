using System;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Diagnostics;

namespace IronText.Framework
{
    using IronText.Extensibility;
    using State = System.Int32;
    using Token = System.Int32;

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
        private readonly Dictionary<Tuple<Token,int>,T> N = new Dictionary<Tuple<Token,int>,T>();
        private readonly T[]                  nodeBuffer;
        private readonly int[]                tokenComplexity;

        private bool                          accepted = false;
        private readonly ILogging             logging;
        private bool isVerifier;

        public RnGlrParser(
            BnfGrammar          grammar,
            TransitionDelegate  transition,
            Token[]             stateToPriorToken,
            int[]               conflictActionsTable,
            IProducer<T>        producer,
            ResourceAllocator   allocator,
            ILogging            logging)
            : this(
                grammar,
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
            TransitionDelegate  transition,
            Token[]             stateToPriorToken,
            int[]               conflictActionsTable,
            IProducer<T>        producer,
            ResourceAllocator   allocator,
            ILogging            logging,
            Gss<T>              gss)
        {
            this.grammar              = grammar;
            this.tokenComplexity      = grammar.GetTokenComplexity();
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
            Reducer(item);

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

            // Plan shifting using the latest version of the GSS front
            foreach (var frontNode in gss.Front)
            {
                var shift = GetShift(frontNode.State, item.Id);
                if (shift.HasValue)
                {
                    Q.Enqueue(new PendingShift(frontNode, shift.Value));
                }
            }

            gss.PushLayer();
            Shifter(item);

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

#if false
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Location = item.Location,
                        Message = "Unexpected token " + grammar.TokenName(item.Id)
                    });
#endif

                return RecoverFromError(item);
            }

            gss.EndEdit();
            this.priorInput = item;
            return this;
        }

        public IReceiver<Msg> Done()
        {
            var eoi = new Msg { Id = BnfGrammar.Eoi };
            if (!object.Equals(priorInput, default(Msg)))
            {
                eoi.Location = priorInput.Location.GetEndLocation();
                eoi.HLocation = priorInput.HLocation.GetEndLocation();
            }

            return Next(eoi);
        }

        private bool IsAccepting(State s)
        {
            int cell = transition(s, BnfGrammar.Eoi);
            return ParserAction.GetKind(cell) == ParserActionKind.Accept;
        }

        // Initialy enqueue shifts and reduces
        private void Actor(Msg item)
        {
            foreach (var w in gss.Front)
            {
                var stateReductions = GetReductions(w.State, item.Id);

                foreach (var red in stateReductions)
                {
                    R.Enqueue(w, red.Rule, red.Size);
                }
            }
        }

        private void Reducer(Msg item)
        {
            N.Clear();

            while (!R.IsEmpty)
            {
                GssReducePath<T> path = R.Dequeue();

                Token X = path.Rule.Left;
                int m = path.Size;

                GssNode<T> u = path.LeftNode;
                State k = u.State;
                State l = NonTermGoTo(k, X);

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
                    // TODO: Performance. Choose better collection
                    if (N.TryGetValue(Tuple.Create(X, c), out currentValue))
                    {
                        z = producer.Merge(currentValue, Λ, (IStackLookback<T>)u);
                    }
                    else
                    {
                        z = Λ;
                    }

                    N[Tuple.Create(X, c)] = z;
                }

                // In deterministic case w should be null
                bool stateAlreadyExists = gss.GetFrontNode(l) != null;
                if (stateAlreadyExists)
                {
                    var newLink = gss.Push(u, l, z);
                    if (newLink != null)
                    {
                        if (m != 0)
                        {
                            var reductions = GetReductions(l, item.Id);
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
                else
                {
                    var newLink = gss.Push(u, l, z);
                    var w = gss.GetFrontNode(l);

                    var reductions = GetReductions(l, item.Id);
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

        private void Shifter(Msg item)
        {
            var z = producer.CreateLeaf(item);
            N[Tuple.Create(item.Id, gss.CurrentLayer)] = z;

            while (Q.Count != 0)
            {
                var shift = Q.Dequeue();

                gss.Push(shift.FrontNode, shift.ToState, z);
            }
        }
        
        private Token NonTermGoTo(State state, Token token)
        {
            var action = ParserAction.Decode(transition(state, token));
            if (action == null || action.Kind != ParserActionKind.Shift)
            {
                throw new InvalidOperationException("Non-term action should be shift");
            }

            return action.State;
        }

        private IEnumerable<ModifiedReduction> GetReductions(State state, Token token)
        {
            var result = new List<ModifiedReduction>();
            int? shift;
            GetParserActions(state, token, out shift, result);
            return result;
        }

        private int? GetShift(State state, Token token)
        {
            int? shift;
            GetParserActions(state, token, out shift, null);
            return shift;
        }

        private bool GetParserActions(State state, Token token, out int? shift, List<ModifiedReduction> reductions)
        {
            shift = null;

            ParserAction action = GetDfaCell(state, token);
            if (action == null)
            {
                return false;
            }

            switch (action.Kind)
            {
                case ParserActionKind.Shift:
                    shift = action.State;
                    break;
                case ParserActionKind.Reduce:
                    if (reductions != null)
                    {
                        var rule = grammar.Rules[action.Rule];
                        reductions.Add(new ModifiedReduction(rule, action.Size));
                    }

                    break;
                case ParserActionKind.Accept:
                    accepted = true;
                    break;
                case ParserActionKind.Conflict:
                    foreach (ParserAction conflictAction in GetConflictActions(action.Value1, action.Value2))
                    {
                        switch (conflictAction.Kind)
                        {
                            case ParserActionKind.Shift:
                                shift = conflictAction.State;
                                break;
                            case ParserActionKind.Reduce:
                                if (reductions != null)
                                {
                                    var crule = grammar.Rules[conflictAction.Rule];
                                    reductions.Add(new ModifiedReduction(crule, conflictAction.Size));
                                }

                                break;
                        }
                    }

                    break;
            }

            return true;
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
            int cell = transition(state, token);
            var result = ParserAction.Decode(cell);
            return result;
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
            else
            {
                gss.Undo(0);
            }

            return result.Next(currentInput);
        }
    }
}
