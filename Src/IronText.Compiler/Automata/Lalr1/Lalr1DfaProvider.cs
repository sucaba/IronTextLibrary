using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Runtime;
using System.Linq;

namespace IronText.Automata.Lalr1
{
    /// <summary>
    /// Lookahead sources:
    /// 1. Spontaneously (for kernels):
    ///     A : C . X D [S] ==> A : C X . D [S]
    /// 2. Propagate (for kernels):
    ///     A : C . X D [#] ==> A : C X . D [#]
    /// 3. By closure (for non-kernel items only):
    ///     A : C . X D [S] ==> X : . E [+firsts(D [S])]
    /// </summary>
    partial class Lalr1DfaProvider : ILrDfa
    {
        private readonly BitSetType TokenSet;

        private readonly GrammarAnalysis grammar;

        private readonly Lr0DfaProvider lr0;

        public Lalr1DfaProvider(GrammarAnalysis grammar)
        {
            this.grammar  = grammar;
            this.TokenSet = grammar.TokenSet;
            this.lr0 = new Lr0DfaProvider(grammar);

            this.States = Build();
        }

        public DotState[] States { get; }

        private DotState[] Build()
        {
            var result = lr0.States;

            FillLookaheads(result);

#if VERBOSE
            PrintStates("Final LALR(1) states:", result);
#endif
            return result;
        }

        private void FillLookaheads(DotState[] states)
        {
            foreach (var state in states)
                foreach (var item in state.Items)
                    item.LA = TokenSet.Mutable();

#if VERBOSE
            PrintStates("LR(0) Kernels:", states);
#endif

            // Apply Algorithm 4.62 to the kernel of each set of LR(0) items and grammar
            // symbol X to determine which lookaheads are spontaneously generated
            // for kernel items in GOTO(I, X), and from which items in I lookaheads
            // are propagated to kernel items in GOTO(I, X).
            states[0].KernelItems[0].LA.Add(PredefinedTokens.Eoi);

            var propagation = BuildSponaneousLookaheadTable(states);

#if VERBOSE
            PrintPropogationTable(states, propogation);
            PrintStates("INIT: Before lookahead propogation", states);
            int pass = 0;
#endif

            bool modified;
            do
            {
                modified = false;
                foreach (var fromPoint in KernelPoints(states))
                {
                    var fromLA = fromPoint.Item.LA;

                    foreach (var toPoint in propagation.Get(fromPoint))
                    {
                        var toLA = toPoint.Item.LA;

                        // TODO: Optimize algorithm by introducing some modification timestamps on items
                        if (!fromLA.IsSubsetOf(toLA))
                        {
#if VERBOSE
                            PrintPropogation(states, itemId, propogatedItemId);
#endif

                            toLA.AddAll(fromLA);
                            modified = true;
                        }
                    }
                }

#if VERBOSE
                PrintStates("PASS #" + ++pass + ": ", states);
#endif
            }
            while (modified);

            // Copy lookaheads from the kernel items to non-kernels
            foreach (var state in states)
            {
                CollectClosureLookaheads(state.Items);
            }
        }

        private static IEnumerable<DotPoint> KernelPoints(DotState[] states) =>
            states
            .SelectMany(state =>
                state
                .KernelItems
                .Select(item =>
                    new DotPoint(state, item)));


        private PropagationTable BuildSponaneousLookaheadTable(DotState[] states)
        {
            var result = new PropagationTable();

            foreach (var fromPoint in KernelPoints(states))
            {
                var J = Closure(
                    new MutableDotItemSet
                    {
                            new DotItem(fromPoint.Item)
                            {
                                LA = TokenSet.Of(PredefinedTokens.Propagated).EditCopy()
                            }
                    });

                foreach (var closedItem in J)
                {
                    foreach (var transition in closedItem.Transitions)
                    {
                        var toPoint = fromPoint.Goto(transition);

                        foreach (var lookahead in closedItem.LA)
                        {
                            if (lookahead == PredefinedTokens.Propagated)
                            {
                                result.Add(fromPoint, toPoint);
                            }
                            else
                            {
                                toPoint.Item.LA.Add(lookahead);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private MutableDotItemSet Closure(IDotItemSet itemSet)
        {
            var result = lr0.Closure(itemSet);
            foreach (var item in result)
            {
                if (item.LA == null)
                    item.LA = TokenSet.Mutable();
            }

            CollectClosureLookaheads(result);

            return result;
        }

        private void CollectClosureLookaheads(IDotItemSet result)
        {
            bool modified;

            // Debug.WriteLine("closured lookeads: item count = {0}", result.Count);

            do
            {
                modified = false;

                foreach (var fromItem in result)
                {
                    foreach (var transition in fromItem.Transitions)
                    {
                        int fromItemNextToken = transition.Token;

                        foreach (var toItem in result)
                        {
                            if (fromItemNextToken == toItem.Outcome)
                            {
                                int countBefore = 0;
                                if (!modified)
                                {
                                    countBefore = toItem.LA.Count;
                                }

                                grammar.AddFirst(transition.CreateNextItem(), toItem.LA);

                                if (!modified)
                                {
                                    modified = toItem.LA.Count != countBefore;
                                }
                            }
                        }
                    }
                }

                if (modified)
                {
                    // Debug.WriteLine("closured lookaheads: extra pass");
                }
            }
            while (modified);
        }

        struct DotPoint
        {
            public DotState State;
            public DotItem  Item;

            public DotPoint(DotState state, DotItem item)
            {
                this.State = state;
                this.Item  = state.GetItem(item.ProductionId, item.Position);
            }

            public DotPoint Goto(DotItemTransition transition) =>
                new DotPoint(
                    State.Goto(transition.Token),
                    transition.CreateNextItem());
        }

        class PropagationTable : Dictionary<DotPoint, List<DotPoint>>
        {
            public void Add(DotPoint from, DotPoint to)
            {
                List<DotPoint> tos;
                if (!TryGetValue(from, out tos))
                {
                    tos = new List<DotPoint>();
                    this.Add(from, tos);
                }

                tos.Add(to);
            }

            public IEnumerable<DotPoint> Get(DotPoint from)
            {
                List<DotPoint> result;
                return TryGetValue(from, out result)
                    ? result
                    : Enumerable.Empty<DotPoint>();
            }
        }
    }
}
