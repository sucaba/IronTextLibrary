using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Reflection;
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

        /// <summary>
        /// Optimized algorithm for building LALR(1) states
        /// </summary>
        private DotState[] Build()
        {
            // 1. Construct the kernels of the sets of LR(0) items for G.
            var result = lr0.States;

            FillLookaheads(result);

            return result;

#if VERBOSE
            PrintStates("Final LALR(1) states:", states);
#endif
        }

        private void FillLookaheads(DotState[] result)
        {
            foreach (var state in result)
                foreach (var item in state.Items)
                    item.LA = TokenSet.Mutable();

#if VERBOSE
            PrintStates("LR(0) Kernels:", lr0states);
#endif

            // 2. Apply Algorithm 4.62 to the kernel of each set of LR(0) items and grammar
            // symbol X to determine which lookaheads are spontaneously generated
            // for kernel items in GOTO(I, X), and from which items in I lookaheads
            // are propagated to kernel items in GOTO(I, X).
            result[0].KernelItems[0].LA.Add(PredefinedTokens.Eoi);

            var propogation = DetermineLookaheads(result);

#if VERBOSE
            PrintPropogationTable(lr0states, propogation);
            PrintStates("INIT: Before lookahead propogation", lr0states);
            int pass = 0;
#endif

            bool lookaheadsPropogated;
            do
            {
                lookaheadsPropogated = false;
                for (int from = 0; from != result.Length; ++from)
                {
                    var kernelSet = result[from].KernelItems;
                    foreach (var item in kernelSet)
                    {
                        List<Tuple<int, int, int>> propogatedItems;
                        var itemId = Tuple.Create(from, item.ProductionId, item.Position);
                        if (propogation.TryGetValue(itemId, out propogatedItems))
                        {
                            foreach (var propogatedItemId in propogatedItems)
                            {
                                var propogatedItem = result[propogatedItemId.Item1].GetItem(propogatedItemId.Item2, propogatedItemId.Item3);

                                // TODO: Optimize algorithm by introducing some modification timestamps on items
                                if (!item.LA.IsSubsetOf(propogatedItem.LA))
                                {
#if VERBOSE
                                    PrintPropogation(lr0states, itemId, propogatedItemId);
#endif

                                    propogatedItem.LA.AddAll(item.LA);
                                    lookaheadsPropogated = true;
                                }
                            }
                        }
                    }
                }

#if VERBOSE
                PrintStates("PASS #" + ++pass + ": ", lr0states);
#endif
            }
            while (lookaheadsPropogated);

            // Copy lookaheads from the kernel items to non-kernels
            foreach (var state in result)
            {
                CollectClosureLookaheads(state.Items);
            }
        }

        /// <summary>
        /// Fills kernel items with "spontaneous" lookaheads and returns table for
        /// lookahead propogation.
        /// </summary>
        private Dictionary<Tuple<int, int, int>, List<Tuple<int, int, int>>> 
            DetermineLookaheads(DotState[] lr0states)
        {
            var result = new Dictionary<Tuple<int, int, int>, List<Tuple<int, int, int>>>();
            for (int from = 0; from != lr0states.Length; ++from)
            {
                var fromState = lr0states[from];
                var fromKernel = fromState.KernelItems;

                foreach (var fromItem in fromKernel)
                {
                    var J = Closure(
                        new MutableDotItemSet 
                        {
                            new DotItem(fromItem)
                            {
                                LA = TokenSet.Of(PredefinedTokens.Propagated).EditCopy()
                            }
                        });

                    foreach (var closedItem in J)
                    {
                        foreach (var transition in closedItem.Transitions)
                        {
                            int X = transition.Token;
                            var gotoXstate = fromState.GetNext(X);
                            var gotoX = gotoXstate.Index;
                            Debug.Assert(gotoX >= 0, "Internal error. Non-existing state.");

                            var nextItemIds = Tuple.Create(gotoX, closedItem.ProductionId, closedItem.Position + 1);

                            foreach (var lookahead in closedItem.LA)
                            {
                                if (lookahead == PredefinedTokens.Propagated)
                                {
                                    List<Tuple<int, int, int>> propogatedItems;
                                    var key = Tuple.Create(from, fromItem.ProductionId, fromItem.Position);
                                    if (!result.TryGetValue(key, out propogatedItems))
                                    {
                                        propogatedItems = new List<Tuple<int, int, int>>();
                                        result[key] = propogatedItems;
                                    }

                                    propogatedItems.Add(nextItemIds);
                                }
                                else
                                {
                                    var nextItem = gotoXstate.GetItem(nextItemIds.Item2, nextItemIds.Item3);
                                    nextItem.LA.Add(lookahead);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private MutableDotItemSet Closure(MutableDotItemSet mutableDotItemSet)
        {
            var result = lr0.Closure(mutableDotItemSet);
            foreach (var item in result)
            {
                if (item.LA == null)
                    item.LA = TokenSet.Mutable();
            }

            CollectClosureLookaheads(result);

            return result;
        }

        private static DotItem GetItem(List<DotState> lr0states, Tuple<int, int, int> stateRulePos)
        {
            var state = lr0states[stateRulePos.Item1];
            return state.GetItem(stateRulePos.Item2, stateRulePos.Item3);
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

                                grammar.AddFirst(transition.GetNextItem(), toItem.LA);

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
    }
}
