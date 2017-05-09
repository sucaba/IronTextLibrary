using IronText.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using IronText.Runtime;
using IronText.Common;
using System.Diagnostics;

namespace IronText.Automata.TurnPlanning
{
    class PlanProvider
    {
        readonly Dictionary<int, Plan>       productionPlans = new Dictionary<int, Plan>();
        readonly Dictionary<int, List<Plan>> nonTermPlans    = new Dictionary<int, List<Plan>>();

        public PlanProvider(Grammar grammar)
        {
            foreach (var production in grammar.Productions)
            {
                var plan = CompileProductionTree(production);

                productionPlans
                    .Add(production.Index, plan);

                 nonTermPlans
                    .GetOrAdd(production.Outcome.Index, () => new List<Plan>())
                    .Add(plan);
            }
        }

        public Plan ForProduction(int productionId)
        {
            return productionPlans[productionId];
        }

        public IEnumerable<Plan> ForTokens(params int[] tokens)
        {
            return tokens
                .Where(nonTermPlans.ContainsKey)
                .SelectMany(token => nonTermPlans[token]);
        }

        private Plan CompileProductionTree(Production root)
        {
            var result = new Plan(root.Outcome.Index);

            CompileComponent(root, result);

            return result;
        }

        private void CompileComponent(Production production, Plan outcome)
        {
            bool hasStackBoundaries =
                production.Original.OutcomeToken != PredefinedTokens.AugmentedStart
                && production.IsTopDown;

            if (hasStackBoundaries)
                outcome.Add(Turn.Enter(production.OutcomeToken));

            foreach (var component in production.ChildComponents)
            {
                CompileComponent((dynamic)component, outcome);
            }

            if (production.Original.IsAugmented)
            {
                outcome.Add(Turn.Acceptance());
            }
            else
            {
                outcome.Add(
                    hasStackBoundaries
                    ? Turn.TopDownReduction(production.Index)
                    : Turn.BottomUpReduction(production.Index));
            }

            if (hasStackBoundaries)
                outcome.Add(Turn.Return(production.OutcomeToken));
        }

        private void CompileComponent(Symbol symbol, Plan outcome)
        {
            outcome.Add(Turn.InputConsumption(symbol.Index));
        }
    }
}
