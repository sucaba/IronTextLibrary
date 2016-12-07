using System.Collections.Generic;

namespace IronText.Automata.TurnPlanning
{
    class TurnNfaTransition
    {
        public TurnNfaTransition(Turn turn, IEnumerable<PlanPosition> nextState)
        {
            Turn         = turn;
            NextPostions = nextState;
        }

        public Turn                      Turn         { get; }

        public IEnumerable<PlanPosition> NextPostions { get; }
    }
}
