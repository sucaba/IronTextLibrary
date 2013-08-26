using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Diagnostics;

namespace IronText.Extensibility
{
    public interface ITdfaData
    {
        IRegularAlphabet Alphabet { get; }

        int StateCount { get; }

        int Start { get; }

        TdfaState GetState(int state);

        IEnumerable<TdfaState> EnumerateStates();

        int AddState(TdfaState state);

        void AddTransition(int from, int symbol, int to);

        void DescribeGraph(IGraphView view);

        void DeleteTransition(int from, int symbol);

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
