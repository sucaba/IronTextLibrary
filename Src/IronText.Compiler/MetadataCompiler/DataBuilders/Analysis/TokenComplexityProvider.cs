using IronText.Algorithm;
using IronText.Misc;
using IronText.Reflection;
using IronText.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace IronText.MetadataCompiler.Analysis
{
    class TokenComplexityProvider
    {
        public TokenComplexityProvider(Grammar grammar)
        {
            Table = grammar.Symbols.CreateCompatibleArray(
                                IndexingConstants.NoIndex);

            var sortedTokens = Graph.TopologicalSort(
                                new [] { PredefinedTokens.AugmentedStart },
                                t => GetDependantTokens(grammar, t))
                                .ToArray();

            for (int i = 0; i != sortedTokens.Length; ++i)
            {
                Table[sortedTokens[i]] = i;
            }
        }

        public int[] Table { get; }

        private static IEnumerable<int> GetDependantTokens(Grammar grammar, int token) =>
            grammar
                .Symbols[token]
                .Productions
                .SelectMany(rule => rule.InputTokens);
    }
}
