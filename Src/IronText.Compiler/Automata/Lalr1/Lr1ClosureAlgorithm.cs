using IronText.Algorithm;
using IronText.MetadataCompiler;
using IronText.MetadataCompiler.Analysis;

namespace IronText.Automata.Lalr1
{
    class Lr1ClosureAlgorithm
    {
        private readonly IBuildtimeGrammar     analysis;
        private readonly Lr0ClosureAlgorithm lr0closure;
        private readonly BitSetType          TokenSet;
        private readonly NullableFirstTables tables;

        public Lr1ClosureAlgorithm(
            IBuildtimeGrammar     analysis,
            Lr0ClosureAlgorithm lr0closure,
            NullableFirstTables tables,
            TokenSetProvider    tokenSetProvider)
        {
            this.analysis   = analysis;
            this.TokenSet   = tokenSetProvider.TokenSet;
            this.lr0closure = lr0closure;
            this.tables     = tables;
        }

        public MutableDotItemSet Apply(IDotItemSet itemSet)
        {
            var result = lr0closure.Apply(itemSet);
            foreach (var item in result)
            {
                if (item.LA == null)
                    item.LA = TokenSet.Mutable();
            }

            CollectClosureLookaheads(result);

            return result;
        }

        public void CollectClosureLookaheads(IDotItemSet result)
        {
            int modified;

            // Debug.WriteLine("closured lookeads: item count = {0}", result.Count);

            do
            {
                modified = 0;

                foreach (var fromItem in result)
                {
                    foreach (var transition in fromItem.GotoTransitions)
                    {
                        int fromItemNextToken = transition.Token;

                        foreach (var toItem in result)
                        {
                            if (fromItemNextToken == toItem.Outcome)
                            {
                                // 1. For [SHIFT token] following transition add FIRST tokens.
                                // 2. If token was nullable non-term then continue with 1
                                var nextItem = transition.CreateNextItem();
                                int added = tables.FillFirsts(
                                                nextItem.RemainingInput,
                                                nextItem.LA,
                                                toItem.LA);
                                modified += added;
                            }
                        }
                    }
                }

                if (modified != 0)
                {
                    // Debug.WriteLine("closured lookaheads: extra pass");
                }
            }
            while (modified != 0);
        }
    }
}
