using IronText.Compiler.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.Lalr1
{
    class Lr0DfaProvider : ILrDfa
    {
        private readonly GrammarAnalysis grammar;

        public Lr0DfaProvider(GrammarAnalysis grammar)
        {
            this.grammar = grammar;

            this.States = Build();
        }

        public DotState[] States { get; }

        private DotState[] Build()
        {
            var result = new List<DotState>();

            var initialItemSet = Closure(new MutableDotItemSet
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
                        .SelectMany(item => item.Transitions)
                        .GroupBy(x => x.Token, x => x.CreateNextItem());

                    foreach (var group in nextItemsByToken)
                    {
                        int token = group.Key;

                        var nextStateItems = Closure(new MutableDotItemSet(group));

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

        public MutableDotItemSet Closure(IDotItemSet itemSet)
        {
            var result = new MutableDotItemSet();
            result.AddRange(itemSet);

            bool modified;

            do
            {
                modified = false;

                foreach (var item in result.EnumerateGrowable())
                {
                    foreach (var transition in item.Transitions)
                    {
                        foreach (var childProd in grammar.GetProductions(transition.Token))
                        {
                            if (result.Add(new DotItem(childProd, 0)))
                            {
                                modified = true;
                            }
                        }
                    }
                }
            }
            while (modified);

            return result;
        }
    }
}
