using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class TurnDfaStateDetails
    {
        public TurnDfaStateDetails(TurnDfaState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            this.State = state;
        }

        public void SetPositions(IEnumerable<PlanPosition> nfaPositions)
        {
            if (nfaPositions == null)
            {
                throw new ArgumentNullException(nameof(nfaPositions));
            }

            this.Positions = nfaPositions;
        }

        public TurnDfaState              State     { get; }

        public IEnumerable<PlanPosition> Positions { get; private set; }

        public IEnumerable<TurnDfaSubstate> KernelSubstates =>
            Positions
            .Where(p => p.IsKernel)
            .Select(p => new TurnDfaSubstate(State, p));

        public IEnumerable<TurnDfaSubstate> Substates =>
            Positions
            .Select(p => new TurnDfaSubstate(State, p));
    }
}
