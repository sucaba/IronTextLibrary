using IronText.Common;
using IronText.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class TurnNfa0Provider
    {
        private readonly PlanProvider plans;

        public TurnNfa0Provider(PlanProvider plans)
        {
            this.plans   = plans;
        }

        public IEnumerable<PlanPosition> Start =>
            WithSubcalls(new[] {
                new PlanPosition(
                    plans.ForProduction(PredefinedTokens.AugmentedStart),
                    0)
                });

        public IEnumerable<TurnNfaTransition> Transitions(IEnumerable<PlanPosition> state) =>
            state
                .GroupBy(
                    position => position.NextTurn,
                    (turn, from) =>
                        new TurnNfaTransition(
                            turn,
                            WithSubcalls(PlanPosition.Nexts(from))));

        public IEnumerable<PlanPosition> Subcalls(IEnumerable<PlanPosition> kernel)
        {
            int[] tokensToConsume = kernel
                .Select(p => p.NextTurn.TokenToConsume)
                .NonNull()
                .Distinct()
                .ToArray();

            return plans
                .ForTokens(tokensToConsume)
                .Select(plan => new PlanPosition(plan, 0));
        }

        public IEnumerable<PlanPosition> WithSubcalls(params PlanPosition[] kernel) =>
            WithSubcalls(kernel);

        public IEnumerable<PlanPosition> WithSubcalls(IEnumerable<PlanPosition> kernel)
        {
            return kernel.Concat(Subcalls(kernel));
        }
    }
}
