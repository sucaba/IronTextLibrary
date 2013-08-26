using System;

namespace IronText.Automata.Lalr1
{
    struct TransitionKey : IEquatable<TransitionKey>
    {
        public readonly int State;
        public readonly int Token;

        public TransitionKey(int state, int token)
        {
            State = state;
            Token = token;
        }

        public bool Equals(TransitionKey other)
        {
            return State == other.State
                && Token == other.Token;
        }

        public override int GetHashCode()
        {
            unchecked { return State + Token; }
        }
    }
}
