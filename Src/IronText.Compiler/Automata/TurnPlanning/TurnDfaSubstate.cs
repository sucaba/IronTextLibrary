using System;
using System.Diagnostics;

namespace IronText.Automata.TurnPlanning
{
    struct TurnDfaSubstate
    {
        private static TurnDfaSubstate Fail = new TurnDfaSubstate(
                                                TurnDfaState.Fail,
                                                default(PlanPosition));
        public TurnDfaSubstate(TurnDfaState owner, PlanPosition planPosition)
        {
            this.Owner        = owner;
            this.PlanPosition = planPosition;
        }

        public TurnDfaState Owner        { get; }

        public PlanPosition PlanPosition { get; }

        public TurnDfaSubstate Next()
        {
            if (PlanPosition.IsDone)
            {
                throw new InvalidOperationException(
                    "Nothing beyond done position.");
            }

            return new TurnDfaSubstate(Owner.GetNext(PlanPosition.NextTurn), PlanPosition.Next());
        }

        public override string ToString() => $"{Owner}: {PlanPosition}";
    }
}
