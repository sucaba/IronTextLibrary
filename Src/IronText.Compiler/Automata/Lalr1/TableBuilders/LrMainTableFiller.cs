using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class LrMainTableFiller
    {
        private readonly DotState[] states;

        public LrMainTableFiller(ILrDfa dfa)
        {
            this.states = dfa.States;
        }

        public void Apply(LrTableBuilder builder)
        {
            foreach (var state in states)
            {
                foreach (var item in state.Items)
                {
                    if (!item.IsReduce)
                    {
                        foreach (var transition in item.Transitions)
                        {
                            builder.AssignAction(
                                state.Index,
                                transition.Token,
                                ParserInstruction.Shift(
                                    state.GetNextIndex(transition.Token)));
                        }
                    }
                    else if (item.IsAugmented)
                    {
                        builder.AssignAction(
                            state.Index,
                            PredefinedTokens.Eoi,
                            ParserInstruction.AcceptAction);
                    }
                    else
                    {
                        var action = ParserInstruction.Reduce(item.ProductionId);

                        foreach (var lookahead in item.LA)
                        {
                            builder.AssignAction(
                                state.Index,
                                lookahead,
                                action);
                        }
                    }
                }
            }
        }
    }
}
