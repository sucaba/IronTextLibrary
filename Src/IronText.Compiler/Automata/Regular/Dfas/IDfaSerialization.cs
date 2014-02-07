using System.Collections.Generic;
using System.Text;

namespace IronText.Automata.Regular
{
    using IronText.Algorithm;
    using State = System.Int32;

    public interface IDfaSerialization
    {
        int StateCount { get; }

        State Start { get; }

        TdfaState GetState(int state);

        IEnumerable<TdfaState> EnumerateStates();

        IEnumerable<IntArrow<int>> EnumerateRealTransitions(TdfaState S);

        void Print(StringBuilder output);
    }
}
