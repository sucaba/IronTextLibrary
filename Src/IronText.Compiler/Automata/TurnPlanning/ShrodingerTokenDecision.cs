using IronText.Collections;

namespace IronText.Automata.TurnPlanning
{
    class ShrodingerTokenDecision : Ambiguous<ShrodingerTokenDecision>
    {
        public ShrodingerTokenDecision(
            int  resolvedToken,
            Turn turn,
            ShrodingerTokenDfaState nextState)
        {
            this.Turn          = turn;
            this.NextState     = nextState;
            this.ResolvedToken = resolvedToken;
        }

        public int  ResolvedToken { get; }

        public Turn Turn          { get; }

        public ShrodingerTokenDfaState NextState { get; }
    }
}
