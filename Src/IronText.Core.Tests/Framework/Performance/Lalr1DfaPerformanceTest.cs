using System;
using System.Linq;
using IronText.Automata.Lalr1;
using IronText.Compiler;
using IronText.Compiler.Analysis;
using IronText.Framework;
using IronText.Reflection;
using NUnit.Framework;
using IronText.MetadataCompiler;

namespace IronText.Tests.Framework.Performance
{
    [TestFixture]
    [Explicit]
    public class Lalr1DfaPerformanceTest
    {
        private Grammar grammar;

        [Test]
        public void Test()
        {
            const int tokenCount     = 50;
            const int productionSize = 10;

            this.grammar = new Grammar();
            var symbols = new Symbol[tokenCount];

            for (int i = 0; i != tokenCount; ++i)
            {
                symbols[i] = grammar.Symbols.Add(i.ToString());
            }

            int last  = tokenCount - productionSize;
            for (int i = 0; i != last; ++i)
            {
                var outcome = symbols[i];
                var input   = symbols.Skip(i + 1).Take(productionSize).ToArray();
                grammar.Productions.Add(outcome, input);
            }

            grammar.Start = symbols[0];
            grammar.BuildIndexes();

            var tokenSetProvider = new TokenSetProvider(grammar);
            var nullableFirstTables = new NullableFirstTables(grammar, tokenSetProvider);
            var analysis = new GrammarAnalysis(grammar);
            var lr0closure = new Lr0ClosureAlgorithm(analysis);
            var lalr1closure = new Lalr1ClosureAlgorithm(
                analysis,
                lr0closure,
                nullableFirstTables,
                tokenSetProvider);
            var target = new Lalr1DfaProvider(
                new Lr0DfaProvider(analysis, lr0closure),
                analysis,
                lalr1closure,
                tokenSetProvider);
        }
    }
}
