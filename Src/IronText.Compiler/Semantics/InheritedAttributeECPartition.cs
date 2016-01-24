using IronText.Algorithm;
using IronText.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IronText.Semantics
{
    internal class InheritedAttributeECPartition
    {
        public InheritedAttributeECPartition(Grammar grammar)
        {
            List<KeyValuePair<int, int>> edges = grammar.GetInheritedCopyRules();

            // Augment copy pairs with reverse direction
            edges.AddRange(
                edges.Select(
                    r => new KeyValuePair<int,int>(r.Value, r.Key)).ToArray());

            ECs = Graph.Cluster(
                Enumerable.Range(0, grammar.InheritedProperties.Count),
                fromProp => from edge in edges
                            where edge.Key == fromProp
                            select edge.Value);

            var incompatibleGroups = grammar.GetIncompatibleInheritedGroups();
            if (ECs.Any(c => IsConflicting(c, incompatibleGroups)))
            {
                ECs = grammar.InheritedProperties
                             .Select(inh => new List<int> { inh.Index })
                             .ToList();
            }
        }

        private static bool IsConflicting(List<int> cluster, IEnumerable<int[]> incompatibleGroups)
        {
            bool result = incompatibleGroups.Any(g => cluster.Count(g.Contains) > 1);
            return result;
        }

        public List<List<int>> ECs { get; private set; }
    }
}
