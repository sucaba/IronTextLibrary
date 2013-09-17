using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Diagnostics;
using IronText.Extensibility;

namespace IronText.Automata.Regular
{
    public interface ITdfaData : IScannerAutomata
    {
        IRegularAlphabet Alphabet { get; }

        int StateCount { get; }

        int Start { get; }

        TdfaState GetState(int state);

        IEnumerable<TdfaState> EnumerateStates();

        int AddState(TdfaState state);

        void AddTransition(int from, int symbol, int to);

        void AddTransition(int from, IntSet symbols, int to);

        void DeleteTransition(int from, int symbol);

        void DeleteTransition(int from, IntSet symbols);

        IEnumerable<TdfaTransition> EnumerateIncoming(int state);
    }

    internal static class TdfaDataExtension
    {
        internal static int IndexOfState(this ITdfaData @this, IntSet positionSet)
        {
            int count = @this.StateCount;
            for (int i = 0; i != count; ++i)
            {
                if (@this.GetState(i).Positions.Equals(positionSet))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
