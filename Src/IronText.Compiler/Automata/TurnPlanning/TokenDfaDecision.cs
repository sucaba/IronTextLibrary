using IronText.Collections;

namespace IronText.Automata.TurnPlanning
{
    class TokenDfaDecision : Ambiguous<TokenDfaDecision>
    {
        public TokenDfaDecision(Turn turn, TokenDfaState nextState)
        {
            this.Turn      = turn;
            this.NextState = nextState;
        }

        public Turn          Turn      { get; }

        public TokenDfaState NextState { get; }
    }
}