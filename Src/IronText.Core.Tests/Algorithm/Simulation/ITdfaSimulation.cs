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

        private static bool TryNextWithTunnels(this ITdfaSimulation automaton, int state, int item, out int nextState)
        {
            int currentState = state;

            do
            {
                if (automaton.TryNext(currentState, item, out nextState))
                {
                    return true;
                }
            }
            while (automaton.Tunnel(currentState, out currentState));

            return false;
        }

        public static IEnumerable<Msg> ScanAll(this ITdfaSimulation automaton, char[] input)
        {
            State state = automaton.Start;
            int acceptPos = -1;
            int startPos  = 0;
            State? acceptingState = null;
            for (int pos = 0; pos != input.Length;)
            {
                var item = input[pos];
                State nextState;

                if (!automaton.TryNextWithTunnels(state, item, out nextState))
                {
                    if (!acceptingState.HasValue)
                    {
                        var msg = string.Format("Scan failed at {0} position.", acceptPos);
                        throw new InvalidOperationException(msg);
                    }

                    int? action = automaton.GetAction(acceptingState.Value);
                    if (action.HasValue)
                    {
                        yield return new Msg(
                                action.Value,
                                new string(input, startPos, (acceptPos - startPos)),
                                null,
                                new Loc(startPos, acceptPos));
                    }

                    startPos = pos = acceptPos;
                }
                else
                {
                    ++pos;
                    state = nextState;
                    if (automaton.IsAccepting(state))
                    {
                        acceptPos      = pos;
                        acceptingState = state;
                    }
                }
            }
        }
    }
}
