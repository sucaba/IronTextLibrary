using System;
using IronText.Automata.Lalr1;
using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework.Performance
{
    [TestFixture]
    [Explicit]
    public class Lalr1DfaPerformanceTest
    {
        private BnfGrammar grammar;

        [Test]
        public void Test()
        {
            const int tokenCount = 50;
            const int ruleSize = 10;

            int[] tokens = new int[tokenCount];
            
            this.grammar = new BnfGrammar();

            for (int i = 0; i != tokenCount; ++i)
            {
                tokens[i] = grammar.DefineToken(i.ToString());
            }

            int iterationCount = tokenCount - ruleSize;
            for (int i = 0; i < iterationCount; ++i)
            {
                int left = tokens[i];
                int[] parts = new int[ruleSize];
                Array.Copy(tokens, i + 1, parts, 0, ruleSize);
                grammar.DefineRule(left, parts);
            }

            grammar.Freeze();
            
            var target = new Lalr1Dfa(grammar, LrTableOptimizations.Default);
        }
    }
}
