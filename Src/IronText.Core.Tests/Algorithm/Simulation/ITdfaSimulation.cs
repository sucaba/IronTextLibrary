using System.Collections.Generic;

namespace IronText.Tests.Algorithm
{
    using IronText.Logging;
    using IronText.Runtime;
    using System;
    using State = System.Int32;

    public interface ITdfaSimulation
    {
        int Start { get; }

        bool TryNext(State state, int input, out State next);

        bool Tunnel(State state, out State next);

        bool IsAccepting(State state);

        int? GetAction(State state);
    }

    public static class TdfaSimulationAlgorithms
    {
        public static bool Match(this ITdfaSimulation automaton, IEnumerable<int> input)
        {
            State state = automaton.Start;
            foreach (var item in input)
            {
                State nextState;
                if (automaton.TryNext(state, item, out nextState))
                {
                    state = nextState;
                }
                else if (automaton.Tunnel(state, out nextState))
                {
                    state = nextState;
                }
                else
                {
                    break;
                }
            }

            return automaton.IsAccepting(state);
        }

        public static bool MatchBeginning(this ITdfaSimulation automaton, int[] input)
        {
            int action;
            return automaton.Scan(input, out action);
        }

        private static bool Scan(this ITdfaSimulation automaton, int[] input, out State action)
        {
            State state = automaton.Start;
            State? acceptingState = null;
            foreach (var item in input)
            {
                State nextState;

                if (automaton.TryNext(state, item, out nextState))
                {
                    state = nextState;
                }
                else if (automaton.Tunnel(state, out nextState))
                {
                    state = nextState;
                }
                else
                {
                    break;
                }

                if (automaton.IsAccepting(state))
                {
                    acceptingState = state;
                }
            }

            if (!acceptingState.HasValue)
            {
                action = -1;
                return false;
            }

            var optAction = automaton.GetAction(acceptingState.Value);
            action = optAction.GetValueOrDefault(-1);
            return true;
        }

        
    }
}
