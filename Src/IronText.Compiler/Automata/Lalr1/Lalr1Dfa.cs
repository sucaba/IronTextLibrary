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
    partial class Lalr1Dfa : ILrDfa
    {
        private readonly BitSetType TokenSet;
        private BitSetType StateSet;

        private readonly GrammarAnalysis grammar;

        private DotState[] states;

        public Lalr1Dfa(GrammarAnalysis grammar, LrTableOptimizations optimizations)
        {
            this.grammar       = grammar;
            this.Optimizations = optimizations;
            this.TokenSet      = grammar.TokenSet;

            BuildLalr1States();

            if ((Optimizations & LrTableOptimizations.EliminateLr0ReduceStates) != 0)
            {
                EliminateLr0ReduceStates();
            }
        }

        public LrTableOptimizations Optimizations { get; private set; }

        private void EliminateLr0ReduceStates()
        {
            var newStates = new List<DotState>();
            var removed = new List<DotState>();

            int index = 0;
            foreach (var state in this.states)
            {
                if (!state.IsReduceState)
                {
                    state.Reindex(index++);
                    newStates.Add(state);
                }
                else
                {
                    removed.Add(state);
                }
            }

#if false
            Debug.WriteLine("Removed {0} LR0-reduce states.", removed.Count);
#endif
            this.states = newStates.ToArray();

            foreach (var state in this.states)
            {
                state.Transitions.RemoveAll(t => removed.Contains(t.To));
            }
        }

        public DotState[] States { get { return this.states; } }

        /// <summary>
        /// Optimized algorithm for building LALR(1) states
        /// </summary>
        private void BuildLalr1States()
        {
            // 1. Construct the kernels of the sets of LR(0) items for G.
            var lr0states = BuildLr0ItemSets();

#if VERBOSE
            PrintStates("LR(0) Kernels:", lr0states);
#endif

            // 2. Apply Algorithm 4.62 to the kernel of each set of LR(0) items and grammar
            // symbol X to determine which lookaheads are spontaneously generated
            // for kernel items in GOTO(I, X), and from which items in I lookaheads
            // are propagated to kernel items in GOTO(I, X).
            lr0states[0].KernelItems[0].LA.Add(PredefinedTokens.Eoi);

            var propogation = DetermineLookaheads(lr0states);

#if VERBOSE
            PrintPropogationTable(lr0states, propogation);
            PrintStates("INIT: Before lookahead propogation", lr0states);
            int pass = 0;
#endif

            bool lookaheadsPropogated;
            do
            {
                lookaheadsPropogated = false;
                for (int from = 0; from != lr0states.Length; ++from)
                {
                    var kernelSet = lr0states[from].KernelItems;
                    foreach (var item in kernelSet)
                    {
                        List<Tuple<int, int, int>> propogatedItems;
                        var itemId = Tuple.Create(from, item.ProductionId, item.Position);
                        if (propogation.TryGetValue(itemId, out propogatedItems))
                        {
                            foreach (var propogatedItemId in propogatedItems)
                            {
                                var propogatedItem = lr0states[propogatedItemId.Item1].GetItem(propogatedItemId.Item2, propogatedItemId.Item3);

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
            foreach (var state in lr0states)
            {
                CollectClosureLookaheads(state.Items, grammar);
            }

            this.states = lr0states;

#if VERBOSE
            PrintStates("Final LALR(1) states:", states);
#endif
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
                    var J = ClosureLr0(
                        new MutableDotItemSet 
                        {
                            new DotItem(fromItem)
                            {
                                LA = TokenSet.Of(PredefinedTokens.Propagated).EditCopy()
                            }
                        });

                    CollectClosureLookaheads(J, grammar);

                    foreach (var closedItem in J)
                    {
                        foreach (var transition in closedItem.Transitions)
                        {
                            int X = transition.Token;
                            var gotoXstate = fromState.GetNext(X);
                            var gotoX = gotoXstate == null ? -1 : gotoXstate.Index;
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

        private static DotItem GetItem(List<DotState> lr0states, Tuple<int, int, int> stateRulePos)
        {
            var state = lr0states[stateRulePos.Item1];
            return state.GetItem(stateRulePos.Item2, stateRulePos.Item3);
        }

        private DotState[] BuildLr0ItemSets()
        {
            var result = new List<DotState>();

            var initialItemSet = ClosureLr0(new MutableDotItemSet
                { 
                    new DotItem(grammar.AugmentedProduction, 0)
                    { 
                        LA = TokenSet.Mutable() 
                    }
                });
            result.Add(new DotState(0, initialItemSet));

            bool addedStatesInRound;

            do
            {
                addedStatesInRound = false;

                for (int i = 0; i != result.Count; ++i)
                {
                    var itemSet = result[i].Items;

                    var nextItemsByToken = itemSet
                        .SelectMany(item => item.Transitions)
                        .GroupBy(x => x.Token, x => x.GetNextItem());

                    foreach (var group in nextItemsByToken)
                    {
                        int token = group.Key;

                        var nextStateItems = ClosureLr0(new MutableDotItemSet(group));

                        CollectClosureLookaheads(nextStateItems, grammar);
                        if (nextStateItems.Count == 0)
                        {
                            throw new InvalidOperationException("Internal error: next state cannot be empty");
                        }

                        var nextState = result.Find(state => state.Items.Equals(nextStateItems));
                        if (nextState == null)
                        {
                            addedStatesInRound = true;
                            nextState = new DotState(result.Count, nextStateItems);
                            result.Add(nextState);
                        }

                        if (result[i].AddTransition(token, nextState, TokenSet))
                        {
                            addedStatesInRound = true;
                        }
                    }
                }
            }
            while (addedStatesInRound);

            StateSet = new BitSetType(result.Count);

            return result.ToArray();
        }

        // TODO: Separate Closure from the lookahead closuring to get cached closure item sets
        private MutableDotItemSet ClosureLr0(MutableDotItemSet itemSet)
        {
            var result = new MutableDotItemSet();
            result.AddRange(itemSet);

            bool modified;

            do
            {
                modified = false;

                // result may grow during iterations
                for (int i = 0; i != result.Count; ++i)
                {
                    var item = result[i];

                    foreach (var transition in item.Transitions)
                    {
                        int X = transition.Token;
                        if (grammar.IsTerminal(X))
                        {
                            continue;
                        }

                        foreach (var childProd in grammar.GetProductions(X))
                        {
                            var newItem = new DotItem(childProd, 0)
                            { 
                                LA = TokenSet.Mutable()
                            };

                            var index = result.IndexOf(newItem);
                            if (index < 0)
                            {
                                result.Add(newItem);
                                modified = true;
                            }
                            else
                            {
                                var existing = result[index];
                                existing.LA.AddAll(newItem.LA);
                            }
                        }
                    }
                }
            }
            while (modified);

            return result;
        }

        // TODO: Performance
        private static void CollectClosureLookaheads(IDotItemSet result, GrammarAnalysis grammar)
        {
            int count = result.Count;
            if (count == 0)
            {
                return;
            }

            
            bool modified;

            // Debug.WriteLine("closured lookeads: item count = {0}", result.Count);

            do
            {
                modified = false;

                for (int i = 0; i != count; ++i)
                {
                    var fromItem = result[i];
                    foreach (var transition in fromItem.Transitions)
                    {
                        int fromItemNextToken = transition.Token;

                        for (int j = 0; j != count; ++j)
                        {
                            var toItem = result[j];

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
