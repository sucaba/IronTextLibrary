using IronText.Compiler.Analysis;
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
                    AssignItemActions(builder, state, item);
                }
            }
        }

        private static void AssignItemActions(
            LrTableBuilder builder,
            DotState state,
            DotItem item)
        {
            if (!item.IsReduce)
            {
                foreach (var itemTransition in item.Transitions)
                {
                    builder.AssignShift(state, itemTransition);
                }
            }
            else if (item.IsAugmented)
            {
                builder.AssignAccept(state);
            }
            else
            {
                foreach (var lookahead in item.LA)
                {
                    builder.AssignReduce(state, item, lookahead);
                }
            }
        }
    }
}
