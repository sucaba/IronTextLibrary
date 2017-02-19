﻿using IronText.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using IronText.Runtime;
using IronText.Common;

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
                var plan = Build(production);

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

        private Plan Build(Production root)
        {
            var result = new Plan(root.Outcome.Index);

            if (root.Original.OutcomeToken == PredefinedTokens.AugmentedStart)
            {
                Compile(root, result);
            }
            else
            {
                result.Add(Turn.Enter(root.OutcomeToken));
                Compile(root, result);
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
