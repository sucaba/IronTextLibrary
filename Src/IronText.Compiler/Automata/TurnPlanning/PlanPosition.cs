using IronText.Common;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using System.Text;

namespace IronText.Automata.TurnPlanning
{
    struct PlanPosition //: IEquatable<PlanPosition>
    {
        public static IEnumerable<PlanPosition> Nexts(IEnumerable<PlanPosition> positions)
            => NotDone(positions)
                .Select(Next);

        public static IEnumerable<PlanPosition> NotDone(IEnumerable<PlanPosition> positions)
            => positions
                .Where(p => !p.IsDone);

        public static PlanPosition Next(PlanPosition position)
            => position.Next();


        private readonly int  _position;

        public PlanPosition(Plan plan, int position)
        {
            if (position > plan.Count)
            {
                throw new ArgumentException(
                    "Position out of range.", nameof(position));
            }

            this.Plan      = plan;
            this._position = position;
        }

        public Plan Plan { get; }

        public int Position => _position;

        public IEnumerable<int> TokensToConsume => Plan.GetTokensToConsume(_position);

        public Turn NextTurn => _position == Plan.Count ? Turn.Unknown() : Plan[_position];

        public bool IsKernel => _position != 0 || Plan.IsAugmentedStart;

        public bool IsSubcall => !IsKernel;

        public bool IsDone => Plan.Count == _position;

        public PlanPosition Next()
        {
            if (IsDone)
            {
                throw new InvalidOperationException("Unable to move beyond a done position.");
            }

            return new PlanPosition(Plan, _position + 1);
        }

        public override string ToString()
        {
            var output = new StringBuilder();
            int count = Plan.Count;
            for (int i = 0; i != count; ++i)
            {
                if (i == _position)
                {
                    output.Append(" \u2022");
                }

                output.Append(' ').Append(Plan[i]);
            }

            if (_position == count)
            {
                output.Append(" \u2022");
            }

            return output.ToString();
        }
    }
}
