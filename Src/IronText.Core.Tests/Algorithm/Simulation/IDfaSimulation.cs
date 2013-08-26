using System.Collections.Generic;

namespace IronText.Tests.Algorithm
{
    public interface IDfaSimulation
    {
        int Start { get; }

        bool TryNext(int state, int input, out int next);

        bool IsAccepting(int state);

        int? GetAction(int state);
    }

    /// <summary>
    /// DFA matching algorithms
    /// </summary>
    public static class DfaSimulationExtensions
    {
        public static bool Match(this IDfaSimulation dfa, IEnumerable<int> input)
        {
            int state = dfa.Start;
            foreach (var item in input)
            {
                if (!dfa.TryNext(state, item, out state))
                {
                    return false;
                }
            }

            return dfa.IsAccepting(state);
        }

        public static bool MatchBeginning(this IDfaSimulation dfa, int[] input)
        {
            int action;
            return dfa.Scan(input, out action);
        }

        public static bool Scan(this IDfaSimulation dfa, int[] input, out int action)
        {
            int state = dfa.Start;
            int? acceptingState = null;
            foreach (var item in input)
            {
                if (!dfa.TryNext(state, item, out state))
                {
                    break;
                }

                if (dfa.IsAccepting(state))
                {
                    acceptingState = state;
                }
            }

            if (!acceptingState.HasValue)
            {
                action = -1;
                return false;
            }

            var optAction = dfa.GetAction(acceptingState.Value);
            action = optAction.GetValueOrDefault(-1);
            return true;
        }
    }
}
