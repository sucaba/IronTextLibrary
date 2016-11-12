using IronText.Compiler.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.Lalr1
{
    class Lr0Nfa
    {
        private readonly GrammarAnalysis grammar;

        public Lr0Nfa(GrammarAnalysis grammar)
        {
            this.grammar = grammar;
        }

        public MutableDotItemSet Start =>
            Closure(new MutableDotItemSet
            {
                new DotItem(
                    grammar.AugmentedProduction,
                    0)
            });

        public IEnumerable<IGrouping<int,DotItem>> Gotos(IEnumerable<DotItem> from)
        {
            return from
                .SelectMany(item => item
                    .GotoTransitions
                    .Select(t =>
                        new { t, next = t.Apply(item) }))
                .GroupBy(x => x.t.Token, x => x.next);
        }

        public MutableDotItemSet Closure(IEnumerable<DotItem> itemSet)
        {
            var result = new MutableDotItemSet(itemSet);

            bool modified;

            do
            {
                modified = false;

                foreach (var item in result.EnumerateGrowable())
                {
                    foreach (var transition in item.GotoTransitions)
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
