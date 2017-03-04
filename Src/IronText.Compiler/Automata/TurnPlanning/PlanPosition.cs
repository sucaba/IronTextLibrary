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
                .Where(p => !p.IsDone)
                .Select(Next)
                .Where(p => !p.IsDone);

        public static PlanPosition Next(PlanPosition position)
            => position.Next();

        private readonly int  _position;

        public PlanPosition(Plan plan, int position)
        {
            this.Plan     = plan;
            this._position = position;
        }

        public Plan Plan { get; }

        public int Position => _position;

        public IEnumerable<int> TokensToConsume => Plan.GetTokensToConsume(_position);

        public Turn NextTurn => Plan[_position];

        public bool IsKernel => _position != 0 || Plan.IsAugmentedStart;

        public bool IsSubcall => !IsKernel;

        public bool IsDone => Plan.Count == _position;

        public PlanPosition Next() => new PlanPosition(Plan, _position + 1);
    }
}
