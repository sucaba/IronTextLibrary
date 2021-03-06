﻿using System;
using IronText.Automata.Lalr1;
using IronText.Compiler;
using IronText.Compiler.Analysis;
using IronText.Framework;
using IronText.Reflection;
using NUnit.Framework;

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
            const int tokenCount = 50;
            const int ruleSize = 10;

            int[] tokens = new int[tokenCount];
            
            this.grammar = new Grammar();

            for (int i = 0; i != tokenCount; ++i)
            {
                tokens[i] = grammar.Symbols.Add(i.ToString()).Index;
            }

            int iterationCount = tokenCount - ruleSize;
            for (int i = 0; i < iterationCount; ++i)
            {
                int outcome = tokens[i];
                int[] pattern = new int[ruleSize];
                Array.Copy(tokens, i + 1, pattern, 0, ruleSize);
                grammar.Productions.Define(outcome, pattern);
            }

            var target = new Lalr1Dfa(new GrammarAnalysis(grammar), LrTableOptimizations.Default);
        }
    }
}
