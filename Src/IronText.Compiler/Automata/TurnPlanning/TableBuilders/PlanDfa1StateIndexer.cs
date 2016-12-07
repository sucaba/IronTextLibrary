using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronText.Automata.TurnPlanning
{
    class PlanDfa1StateIndexer
    {
        Dictionary<TurnDfaState, int> stateToIndex = new Dictionary<TurnDfaState, int>();
        TurnDfaState[] indexToState;

        public PlanDfa1StateIndexer(TurnDfa1Provider dfa)
        {
            indexToState = dfa.States.ToArray();
            int count = indexToState.Length;
            for (int i = 0; i != count; ++i)
            {
                stateToIndex[indexToState[i]] = i;
            }
        }

        public int Get(TurnDfaState state) => stateToIndex[state];
    }
}
