using IronText.Common;

namespace IronText.Automata.TurnPlanning
{
    class MainTurnTableFiller
    {
        private readonly TokenDfaState[]        states;
        private readonly Indexer<TokenDfaState> stateIndexer;
        private readonly TurnConflictResolver   conflictResolver;

        public MainTurnTableFiller(
            TokenDfaProvider       dfa, 
            Indexer<TokenDfaState> stateIndexer,
            TurnConflictResolver   conflictResolver)
        {
            this.states            = dfa.States;
            this.stateIndexer      = stateIndexer;
            this.conflictResolver  = conflictResolver;
        }

        public void Apply(TurnTableBuilder builder)
        {
            foreach (var state in states)
            {
                var transitions = conflictResolver.PrioritizeBy(state.Transitions, x => x.Value.Turn);
                foreach (var t in transitions)
                {
                    AssignAction(builder, state, (dynamic)t.Value.Turn, t.Key);
                }
            }
        }

        private void AssignAction(
            TurnTableBuilder     builder,
            TokenDfaState        state,
            InputConsumptionTurn turn,
            int                  lookahead)
        {
            builder.AssignShift(
                stateIndexer[state],
                turn.Token,
                stateIndexer[state.GetNext(turn)]);
        }

        private void AssignAction(
            TurnTableBuilder builder,
            TokenDfaState    state,
            AcceptanceTurn   turn,
            int              lookahead)
        {
            builder.AssignAccept(stateIndexer[state]);
        }

        private void AssignAction(
            TurnTableBuilder   builder,
            TokenDfaState      state,
            InnerReductionTurn turn,
            int                lookahead)
        {
            builder.AssignReduce(
                stateIndexer[state],
                lookahead,
                turn.ProductionId,
                stateIndexer[state.GetNext(turn)]);
        }

        private void AssignReturn(
            TurnTableBuilder    builder,
            TokenDfaState       state,
            ReturnTurn          turn,
            int                 lookahead)
        {
            builder.AssignReturn(
                stateIndexer[state],
                lookahead,
                turn.ProducedToken);
        }
    }
}
