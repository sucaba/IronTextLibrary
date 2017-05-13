using IronText.Common;

namespace IronText.Automata.TurnPlanning
{
    class TurnDfa1Provider
    {
        public TurnDfaState[]                  States             { get; }

        public TokenSetsRelation<TurnDfaState> ReturnLookaheads   { get; }

        public ImplMap<TurnDfaState, TurnDfaStateDetails> Details { get; }

        public TurnDfa1Provider(
            TurnDfa0Provider        dfa0,
            DonePositionLookaheadProvider returnLookaheadProvider)
        {
            this.States           = dfa0.States;
            this.Details          = dfa0.Details;
            this.ReturnLookaheads = returnLookaheadProvider.StateLookaheads;
        }
    }
}
