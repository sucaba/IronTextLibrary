using IronText.Automata.DotNfa;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.Lalr1
{
    class Lr0DfaProvider
    {
        private readonly IBuildtimeGrammar grammar;
        private readonly Lr0ClosureAlgorithm closure;

        public Lr0DfaProvider(
            IBuildtimeGrammar grammar,
            Lr0ClosureAlgorithm closure)
        {
            this.grammar = grammar;
            this.closure = closure;

            this.States = Build();
        }

        public DotState[] States { get; }

        private DotState[] Build()
        {
            var result = new List<DotState>();

            var initialItemSet = closure.Apply(new MutableDotItemSet
                {
                    new DotItem(grammar.AugmentedProduction, 0)
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
                        .SelectMany(item => item.GotoTransitions)
                        .GroupBy(x => x.Token, x => x.CreateNextItem());

                    foreach (var group in nextItemsByToken)
                    {
                        int token = group.Key;

                        var nextStateItems = closure.Apply(new MutableDotItemSet(group));
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

                        if (result[i].AddGoto(token, nextState))
                        {
                            addedStatesInRound = true;
                        }
                    }
                }
            }
            while (addedStatesInRound);

            return result.ToArray();
        }
    }
}
