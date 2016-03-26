using IronText.Algorithm;
using IronText.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IronText.Semantics
{
    internal class InheritedPropertyECCollection 
        : GrammarEntityCollection<InheritedPropertyEC,IGrammarScope>
    {
        public InheritedPropertyECCollection(Grammar grammar)
            : base(grammar)
        {
            List<Edge> edges = GetInheritedCopyRules();

            // Augment copy pairs with reverse direction
            edges.AddRange(edges.Select(e => e.Opposite).ToArray());

            var ecs = Graph.Cluster(
                Enumerable.Range(0, Scope.InheritedProperties.Count),
                fromProp => from edge in edges
                            where edge.From == fromProp
                            select edge.To);

            var incompatibleGroups = GetIncompatibleInheritedGroups();
            if (ecs.Any(c => HasConflictingPairs(c, incompatibleGroups)))
            {
                foreach (var inh in Scope.InheritedProperties)
                {
                    Add(new InheritedPropertyEC { inh.Index });
                }
            }
            else
            {
                foreach (var ec in ecs)
                {
                    var item = new InheritedPropertyEC();
                    item.AddRange(ec);
                    Add(item);
                }
            }

            BuildIndexes();
        }

        private static bool HasConflictingPairs(List<int> cluster, IEnumerable<int[]> incompatibleGroups)
        {
            bool result = incompatibleGroups.Any(g => HasConflictingPairsFromGroup(cluster, g));
            return result;
        }

        private static bool HasConflictingPairsFromGroup(List<int> cluster, int[] group)
        {
            return cluster.Where(group.Contains).Take(2).Count() > 1;
        }

        private List<Edge> GetInheritedCopyRules()
        {
            var result = new List<Edge>();
            foreach (var prod in Scope.Productions)
            {
                foreach (SemanticFormula formula in prod.Semantics)
                {
                    if (formula.IsCopy)
                    {
                        var inheritedFrom = Scope.InheritedProperties.ResolveInherited(prod, formula.Arguments[0]);
                        var inheritedTo = Scope.InheritedProperties.ResolveInherited(prod, formula.Lhe);
                        if (inheritedFrom != null && inheritedTo != null)
                        {
                            result.Add(new Edge(inheritedFrom.Index, inheritedTo.Index));
                        }
                    }
                }
            }

            return result;
        }

        private IEnumerable<int[]> GetIncompatibleInheritedGroups()
        {
            var result = Scope.InheritedProperties
                        .GroupBy(inh => inh.Symbol)
                        .Select(g => g.Select(inh => inh.Index)
                                      .ToArray());
            return result;
        }
    }
}
