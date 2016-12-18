using IronText.Common;
using System.Collections.Generic;
using System;

namespace IronText.Automata.TurnPlanning
{
    class TokenDfaState
    {
        public static TokenDfaState FailState { get; } = new TokenDfaState();

        public Dictionary<int, TokenDecision> Transitions { get; }

        public TokenDfaState()
            : this(new Dictionary<int, TokenDecision>())
        {
        }

        private TokenDfaState(Dictionary<int, TokenDecision> transitions)
        {
            this.Transitions = transitions;
        }

        public TokenDecision GetDecision(int token) =>
            Transitions.GetOrDefault(token);

        public TokenDfaState GetNext(int token) =>
            Transitions.GetOrDefault(token)?.NextState ?? FailState;
    }
}
