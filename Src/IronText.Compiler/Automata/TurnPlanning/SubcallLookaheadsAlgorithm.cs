using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;

namespace IronText.Automata.TurnPlanning
{
    class SubcallLookaheadsAlgorithm
    {
        private readonly NullableFirstTables tables;
        private readonly BitSetType tokenSet;

        public SubcallLookaheadsAlgorithm(
            NullableFirstTables tables,
            TokenSetProvider    tokenSetProvider)
        {
            this.tables   = tables;
            this.tokenSet = tokenSetProvider.TokenSet;
        }

        public void Fill(
            IEnumerable<PlanPosition> planPositions,
            TokenSetsRelation<PlanPosition>  lookaheads)
        {
            int modified;

            do
            {
                modified = 0;

                foreach (var from in planPositions)
                {
                    var token = from.NextTurn.TokenToConsume;
                    if (!token.HasValue)
                    {
                        continue;
                    }

                    var next = from.Next();
                    foreach (var to in planPositions.Where(SubcallOf(token.Value)))
                    {
                        int added = tables.FillFirsts(
                                        next.TokensToConsume,
                                        lookaheads.Of(from),
                                        lookaheads.GetMutable(to));
                        modified += added;
                    }
                }
            }
            while (modified != 0);
        }

        private static Func<PlanPosition, bool> SubcallOf(int token) =>
            pos => pos.IsSubcall && pos.Plan.Outcome == token;
    }
}
