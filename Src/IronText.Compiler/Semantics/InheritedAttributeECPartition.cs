using IronText.Algorithm;
using IronText.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IronText.Semantics
{
    internal class InheritedAttributeECPartition
    {
        private readonly Grammar grammar;

        public InheritedAttributeECPartition(Grammar grammar)
        {
            this.grammar = grammar; 

            List<Edge> edges = GetInheritedCopyRules();

            // Augment copy pairs with reverse direction
            edges.AddRange(edges.Select(e => e.Opposite).ToArray());

            ECs = Graph.Cluster(
                Enumerable.Range(0, grammar.InheritedProperties.Count),
                fromProp => from edge in edges
                            where edge.From == fromProp
                            select edge.To);

            var incompatibleGroups = GetIncompatibleInheritedGroups();
            if (ECs.Any(c => HasConflictingPairs(c, incompatibleGroups)))
            {
                ECs = grammar.InheritedProperties
                             .Select(inh => new List<int> { inh.Index })
                             .ToList();
            }
        }

        public List<List<int>> ECs { get; private set; }

        private static bool HasConflictingPairs(List<int> cluster, IEnumerable<int[]> incompatibleGroups)
        {
            bool result = incompatibleGroups.Any(g => HasConflictingPairsFromGroup(cluster, g));
            return result;
        }

        private static bool HasConflictingPairsFromGroup(List<int> cluster, int[] group)
        {
            return cluster.Where(group.Contains).Take(2).Count() > 1;
        }

        internal List<Edge> GetInheritedCopyRules()
        {
            var result = new List<Edge>();
            foreach (var prod in grammar.Productions)
            {
                foreach (SemanticFormula formula in prod.Semantics)
                {
                    if (formula.IsCopy)
                    {
                        result.Add(new Edge(
                            grammar.InheritedProperties.Resolve(prod, formula.ActualRefs[0]).Index,
                            grammar.InheritedProperties.Resolve(prod, formula.Lhe).Index));
                    }
                }
            }

            return result;
        }

        internal IEnumerable<int[]> GetIncompatibleInheritedGroups()
        {
            var result = grammar.InheritedProperties.GroupBy(inh => inh.Symbol)
                        .Select(g => g.Select(inh => inh.Index).ToArray());
            return result;
        }
    }
}
