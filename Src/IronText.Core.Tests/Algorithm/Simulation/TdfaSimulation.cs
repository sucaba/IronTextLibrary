using IronText.Extensibility;

namespace IronText.Tests.Algorithm
{
    internal class TdfaSimulation : ITdfaSimulation
    {
        private readonly ITdfaData data;

        public TdfaSimulation(ITdfaData data)
        {
            this.data = data;
        }

        public int Start { get { return 0; } }

        public bool TryNext(int state, int input, out int next)
        {
            int symbol = data.Alphabet.Encode(input);

            var stateInfo = data.GetState(state);
            foreach (var transition in stateInfo.Outgoing)
            {
                if (transition.Symbols.Contains(symbol))
                {
                    next = transition.To;
                    return true;
                }
            }

            next = -1;
            return false;
        }

        public bool Tunnel(int state, out int next)
        {
            var stateInfo = data.GetState(state);
            if (stateInfo.Tunnel >= 0)
            {
                next = stateInfo.Tunnel;
                return true;
            }

            next = -1;
            return false;
        }

        public bool IsAccepting(int state)
        {
            return data.GetState(state).IsAccepting;
        }

        public int? GetAction(int state)
        {
            return data.GetState(state).Action;
        }
    }
}
