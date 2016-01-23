using IronText.Algorithm;
using IronText.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Semantics
{
    internal class InheritedAttributeECPartition
    {
        public InheritedAttributeECPartition(Grammar grammar)
        {
            List<KeyValuePair<int, int>> edges = grammar.GetInheritedCopyRules();

            // Augment copy pairs with reverse directions 
            // to properly represent an undirected graph.
            edges.AddRange(
                edges.Select(
                    r => new KeyValuePair<int,int>(r.Value, r.Key)).ToArray());

            ECs = Graph.Cluster(
                Enumerable.Range(0, grammar.InheritedProperties.Count),
                fromProp => from edge in edges
                            where edge.Key == fromProp
                            select edge.Value);
        }

        public List<List<int>> ECs { get; private set; }
    }
}
