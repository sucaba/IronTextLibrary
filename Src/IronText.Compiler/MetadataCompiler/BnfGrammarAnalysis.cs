using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Algorithm;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Prebuilds various tables related to <see cref="IronText.Framework.BnfGrammar"/>
    /// </summary>
    sealed class BnfGrammarAnalysis
    {
        private readonly BnfGrammar grammar;

        public BnfGrammarAnalysis(BnfGrammar grammar)
        {
            this.grammar = grammar;
        }

        /// <summary>
        /// Fewer values are less dependent to higher values 
        /// Relation of values is non-determined for two mutally 
        /// dependent non-terms.
        /// </summary>
        public int[] GetTokenComplexity()
        {
            var result = Enumerable.Repeat(-1, grammar.TokenCount).ToArray();
            var sortedTokens = Graph.ToplogicalSort(
                                new [] { BnfGrammar.AugmentedStart },
                                GetDependantTokens)
                                .ToArray();
            for (int i = 0; i != sortedTokens.Length; ++i)
            {
                result[sortedTokens[i]] = i;
            }

            return result;
        }

        private IEnumerable<int> GetDependantTokens(int token)
        {
            foreach (var rule in grammar.GetProductionRules(token))
            {
                foreach (int part in rule.Parts)
                {
                    yield return part;
                }
            }
        }
    }
}
