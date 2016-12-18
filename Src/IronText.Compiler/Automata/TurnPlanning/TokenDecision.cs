using IronText.Collections;

namespace IronText.Automata.TurnPlanning
{
    class TokenDecision : Ambiguous<TokenDecision>
    {
        public TokenDecision(Turn turn, TokenDfaState nextState)
        {
            this.Turn      = turn;
            this.NextState = nextState;
        }

        public Turn          Turn      { get; }

        public TokenDfaState NextState { get; }
    }
}