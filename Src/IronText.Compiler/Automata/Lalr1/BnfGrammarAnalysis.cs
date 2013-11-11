using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Algorithm;
using IronText.Framework.Reflection;

namespace IronText.Automata.Lalr1
{
    /// <summary>
    /// Prebuilds various tables related to <see cref="IronText.Framework.BnfGrammar"/>
    /// </summary>
    sealed class BnfGrammarAnalysis
    {
        private readonly IBuildtimeBnfGrammar grammar;

        public BnfGrammarAnalysis(IBuildtimeBnfGrammar grammar)
        {
            this.grammar = grammar;
        }

        public IBuildtimeBnfGrammar Grammar { get { return grammar; } } 

        /// <summary>
        /// Fewer values are less dependent to higher values 
        /// Relation of values is non-determined for two mutally 
        /// dependent non-terms.
        /// </summary>
        public int[] GetTokenComplexity()
        {
            var result = Enumerable.Repeat(-1, grammar.SymbolCount).ToArray();
            var sortedTokens = Graph.ToplogicalSort(
                                new [] { EbnfGrammar.AugmentedStart },
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
            foreach (var rule in grammar.GetProductions(token))
            {
                foreach (int part in rule.Pattern)
                {
                    yield return part;
                }
            }
        }
    }
}
