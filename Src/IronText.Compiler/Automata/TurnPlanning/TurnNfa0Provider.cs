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
            WithSubcalls(
                plans
                .ForTokens(PredefinedTokens.AugmentedStart)
                .Select(plan => new PlanPosition(plan, 0))
                .ToArray()
            );

        public IEnumerable<TurnNfaTransition> Transitions(IEnumerable<PlanPosition> state) =>
            state
                .Where(s => !s.IsDone)
                .GroupBy(
                    position => position.NextTurn,
                    (turn, from) =>
                        new TurnNfaTransition(
                            turn,
                            WithSubcalls(PlanPosition.Nexts(from))));

        private IEnumerable<PlanPosition> Subcalls(IEnumerable<PlanPosition> kernel)
        {
            return Subcalls(kernel, excludedTokens: new int[0]);
        }

        private IEnumerable<PlanPosition> Subcalls(IEnumerable<PlanPosition> kernel, IEnumerable<int> excludedTokens)
        {
            int[] tokensToConsume = PlanPosition.NotDone(kernel)
                .Select(p => p.NextTurn.TokenToConsume)
                .NonNull()
                .Distinct()
                .Except(excludedTokens)
                .ToArray();

            return Subcalls(tokensToConsume, excludedTokens);
        }

        private IEnumerable<PlanPosition> Subcalls(int[] tokensToConsume, IEnumerable<int> excludedTokens)
        {
            var positions = plans
                .ForTokens(tokensToConsume.Except(excludedTokens))
                .Select(plan => new PlanPosition(plan, 0));
            if (positions.Any())
            {
                return positions.Concat(
                    Subcalls(positions, excludedTokens: tokensToConsume.Union(excludedTokens)));
            }

            return positions;
        }

        public IEnumerable<PlanPosition> WithSubcalls(params PlanPosition[] kernel) =>
            WithSubcalls((IEnumerable<PlanPosition>)kernel);

        public IEnumerable<PlanPosition> WithSubcalls(IEnumerable<PlanPosition> kernel)
        {
            return kernel.Concat(Subcalls(kernel));
        }
    }
}
