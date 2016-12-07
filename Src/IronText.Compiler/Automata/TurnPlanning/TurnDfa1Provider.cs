namespace IronText.Automata.TurnPlanning
{
    class TurnDfa1Provider
    {
        public TurnDfaState[]                  States           { get; }

        public TokenSetsRelation<TurnDfaState> ReturnLookaheads { get; }

        public TurnDfa1Provider(
            TurnDfa0Provider        dfa0,
            ReturnLookaheadProvider returnLookaheadProvider)
        {
            this.States           = dfa0.States;
            this.ReturnLookaheads = returnLookaheadProvider.ReturnLookaheads;
        }
    }
}
