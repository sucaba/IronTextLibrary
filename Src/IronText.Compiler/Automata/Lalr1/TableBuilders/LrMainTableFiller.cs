using IronText.Automata.DotNfa;

namespace IronText.Automata.Lalr1
{
    class LrMainTableFiller
    {
        private readonly DotState[] states;

        public LrMainTableFiller(ILrDfa dfa, IBuildtimeGrammar grammar)
        {
            this.states = dfa.States;
            this.SymbolColumnCount = grammar.SymbolCount;
        }

        public int SymbolColumnCount { get; }

        public void Apply(LrTableBuilder builder)
        {
            foreach (var state in states)
            {
                foreach (var item in state.Items)
                {
                    foreach (var transition in item.AllTransitions)
                    {
                        AssignAction(builder, state, (dynamic)transition);
                    }
                }
            }
        }

        private static void AssignAction(
            LrTableBuilder builder,
            DotState state,
            DotItemGotoTransition transition)
        {
            builder.AssignShift(
                state.Index,
                transition.Token,
                state.Goto(transition.Token).Index);
        }

        private static void AssignAction(
            LrTableBuilder builder,
            DotState state,
            DotItemAcceptTransition transition)
        {
            builder.AssignAccept(state.Index);
        }

        private static void AssignAction(
            LrTableBuilder builder,
            DotState state,
            DotItemReduceTransition transition)
        {
            builder.AssignReduce(state.Index, transition.Token, transition.ProductionId);
        }
    }
}
