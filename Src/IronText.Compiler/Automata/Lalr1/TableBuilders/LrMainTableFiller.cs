using System;
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
