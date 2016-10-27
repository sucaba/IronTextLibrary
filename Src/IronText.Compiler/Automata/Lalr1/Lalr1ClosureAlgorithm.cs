using IronText.Compiler.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronText.Algorithm;
using IronText.MetadataCompiler;

namespace IronText.Automata.Lalr1
{
    class Lalr1ClosureAlgorithm
    {
        private readonly GrammarAnalysis     analysis;
        private readonly Lr0ClosureAlgorithm lr0closure;
        private readonly BitSetType          TokenSet;
        private readonly NullableFirstTables tables;

        public Lalr1ClosureAlgorithm(
            GrammarAnalysis     analysis,
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
            bool modified;

            // Debug.WriteLine("closured lookeads: item count = {0}", result.Count);

            do
            {
                modified = false;

                foreach (var fromItem in result)
                {
                    foreach (var transition in fromItem.GotoTransitions)
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

                                // 1. For [SHIFT token] following transition add FIRST tokens.
                                // 2. If token was nullable non-term then continue with 1
                                var nextItem = transition.CreateNextItem();
                                int[] inputTokens = analysis.GetProduction(nextItem.ProductionId).Input;
                                tables.AddFirst(inputTokens.Skip(nextItem.Position), nextItem.LA, toItem.LA);

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
