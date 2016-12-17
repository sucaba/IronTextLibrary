using System;
using System.Collections.Generic;

namespace IronText.Automata.TurnPlanning
{
    class TokenDfaState
    {
        public Dictionary<int, TokenDfaDecision> Transitions { get; }
            = new Dictionary<int, TokenDfaDecision>();

        public TokenDfaState GetNext(Turn turn)
        {
            throw new NotImplementedException();
        }
    }
}
