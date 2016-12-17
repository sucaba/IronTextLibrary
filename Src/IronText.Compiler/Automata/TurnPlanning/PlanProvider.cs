using IronText.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using IronText.Runtime;

namespace IronText.Automata.TurnPlanning
{
    class PlanProvider
    {
        readonly Dictionary<int, Plan> productionPlans = new Dictionary<int, Plan>();
        readonly Dictionary<int, List<Plan>> nonTermPlans = new Dictionary<int, List<Plan>>();

        public PlanProvider(Grammar grammar)
        {
            foreach (var production in grammar.Productions)
            {
                Plan productionPlan = Build(production);
                productionPlans.Add(production.Index, productionPlan);

                List<Plan> outcomePlans;
                if (!nonTermPlans.TryGetValue(production.Outcome.Index, out outcomePlans))
                {
                    outcomePlans = new List<Plan>();
                }

                outcomePlans.Add(productionPlan);
            }
        }

        public Plan ForProduction(int productionId)
        {
            return productionPlans[productionId];
        }

        public IEnumerable<Plan> ForTokens(int[] tokens)
        {
            return tokens
                .Where(nonTermPlans.ContainsKey)
                .SelectMany(token => nonTermPlans[token]);
        }

        private Plan Build(Production root)
        {
            var result = new Plan(root.Outcome.Index);

            Compile(root, result);

            if (root.Original.OutcomeToken != PredefinedTokens.AugmentedStart)
            {
                result.Add(Turn.Return(root.OutcomeToken));
            }

            return result;
        }

        private void Compile(Production production, Plan outcome)
        {
            foreach (var component in production.ChildComponents)
            {
                Compile((dynamic)component, outcome);
            }

            if (production.Original.IsAugmented)
            {
                outcome.Add(Turn.Acceptance());
            }
            else
            {
                outcome.Add(Turn.InnerReduction(production.Index));
            }
        }

        private void Compile(Symbol symbol, Plan outcome)
        {
            outcome.Add(Turn.InputConsumption(symbol.Index));
        }
    }
}
