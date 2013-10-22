using IronText.Extensibility;
using IronText.Automata.Regular;

namespace IronText.Tests.Algorithm
{
    public class DfaSimulation : IDfaSimulation
    {
        private readonly ITdfaData data;

        public DfaSimulation(ITdfaData data)
        {
            this.data = data;
        }

        int IDfaSimulation.Start { get { return 0; } }

        bool IDfaSimulation.TryNext(int state, int input, out int next)
        {
            int symbol = data.Alphabet.Encode(input);
            var stateInfo = data.GetState(state);

            foreach (var t in stateInfo.Outgoing)
            {
                if (t.Symbols.Contains(symbol))
                {
                    next = t.To;
                    return true;
                }
            }

            next = -1;
            return false;
        }

        bool IDfaSimulation.IsAccepting(int state) { return data.GetState(state).IsAccepting; }

        int? IDfaSimulation.GetAction(int state) 
        { 
            var s = data.GetState(state);
            if (s.Actions.Count == 0)
            {
                return null;
            }

            return s.Actions[0];
        }
    }
}
