using IronText.Automata.DotNfa;

namespace IronText.Automata.Lalr1
{
    class Lr0ClosureAlgorithm
    {
        private readonly IBuildtimeGrammar analysis;

        public Lr0ClosureAlgorithm(IBuildtimeGrammar analysis)
        {
            this.analysis = analysis;
        }

        public MutableDotItemSet Apply(IDotItemSet itemSet)
        {
            var result = new MutableDotItemSet();
            result.AddRange(itemSet);

            bool modified;

            do
            {
                modified = false;

                foreach (var item in result.EnumerateGrowable())
                {
                    foreach (var transition in item.GotoTransitions)
                    {
                        foreach (var childProd in analysis.GetProductions(transition.Token))
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
