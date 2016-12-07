namespace IronText.Automata.TurnPlanning
{
    class MainTurnTableFiller
    {
        private readonly TurnDfaState[]                  states;
        private readonly PlanDfa1StateIndexer            stateIndexer;
        private readonly TurnConflictResolver            conflictResolver;
        private readonly TokenSetsRelation<TurnDfaState> firsts;

        public MainTurnTableFiller(
            TurnDfa1Provider       dfa, 
            PlanDfa1StateIndexer   stateIndexer,
            TurnConflictResolver   conflictResolver,
            TurnDfa1FirstsProvider firstsProvider)
        {
            this.states            = dfa.States;
            this.firsts            = firstsProvider.Firsts;
            this.stateIndexer      = stateIndexer;
            this.conflictResolver  = conflictResolver;
        }

        public void Apply(TurnTableBuilder builder)
        {
            foreach (var state in states)
            {
                var lookaheads = firsts.Of(state);

                foreach (var turn in conflictResolver.Prioritize(state.Turns))
                {
                    foreach (var lookahead in lookaheads)
                    {
                        AssignAction(
                            builder,
                            state,
                            (dynamic)turn,
                            lookahead);
                    }
                }
            }
        }

        private void AssignAction(
            TurnTableBuilder     builder,
            TurnDfaState         state,
            InputConsumptionTurn turn,
            int                  lookahead)
        {
            builder.AssignShift(
                stateIndexer.Get(state),
                turn.Token,
                stateIndexer.Get(state.GetNext(turn)));
        }

        private void AssignAction(
            TurnTableBuilder builder,
            TurnDfaState     state,
            AcceptanceTurn   turn,
            int              lookahead)
        {
            builder.AssignAccept(stateIndexer.Get(state));
        }

        private void AssignAction(
            TurnTableBuilder   builder,
            TurnDfaState       state,
            InnerReductionTurn turn,
            int                lookahead)
        {
            builder.AssignReduce(
                stateIndexer.Get(state),
                lookahead,
                turn.ProductionId,
                stateIndexer.Get(state.GetNext(turn)));
        }

        private void AssignReturn(
            TurnTableBuilder    builder,
            TurnDfaState        state,
            ReturnTurn          turn,
            int                 lookahead)
        {
            builder.AssignReturn(
                stateIndexer.Get(state),
                lookahead,
                turn.ProducedToken);
        }
    }
}
