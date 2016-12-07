using IronText.Common;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IronText.Automata.TurnPlanning
{
    struct PlanPosition //: IEquatable<PlanPosition>
    {
        public static IEnumerable<PlanPosition> Nexts(IEnumerable<PlanPosition> positions)
            => positions
                .Select(Next)
                .Where(p => !p.IsDone);

        public static PlanPosition Next(PlanPosition position)
            => position.Next();

        private readonly int  position;

        public PlanPosition(Plan plan, int position)
        {
            this.Plan     = plan;
            this.position = position;
        }

        public Plan Plan { get; }

        public IEnumerable<int> TokensToConsume => Plan.GetTokensToConsume(position);

        public Turn NextTurn => Plan[position];

        public bool IsKernel => position != 0 || Plan.IsAugmentedStart;

        public bool IsSubcall => !IsKernel;

        public bool IsDone => Plan.Count == position;

        public PlanPosition Next() => new PlanPosition(Plan, position + 1);
    }
}
