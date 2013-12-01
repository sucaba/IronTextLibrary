using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Analysis;
using IronText.Framework;
using IronText.Framework.Reflection;
using NUnit.Framework;

namespace IronText.Tests.Analysis
{
    [TestFixture]
    public class ProductionInlinerTest
    {
        private EbnfGrammar originalGrammar;
        private EbnfGrammar resultGrammar;

        private Symbol start;
        private Symbol prefix;
        private Symbol suffix;
        private Symbol inlinedNonTerm;
        private Symbol term1;
        private Symbol term2;
        private Symbol term3;
        private Symbol term4;
        private Symbol term5;
        private Symbol nestedNonTerm;

        [SetUp]
        public void SetUp()
        {
            this.originalGrammar = new EbnfGrammar();
            var symbols = originalGrammar.Symbols;
            this.start  = symbols.Add("start");
            this.prefix = symbols.Add("prefix");
            this.suffix = symbols.Add("suffix");
            this.term1  = symbols.Add("term1");
            this.term2  = symbols.Add("term2");
            this.term3  = symbols.Add("term3");
            this.term4  = symbols.Add("term4");
            this.term5  = symbols.Add("term5");
            this.inlinedNonTerm = symbols.Add("inlinedNonTerm");
            this.nestedNonTerm = symbols.Add("nestedNonTerm");

            originalGrammar.Start = start;

            originalGrammar.Productions.Add(start,  new[] { prefix, inlinedNonTerm, suffix });
        }

        [TearDown]
        public void TearDown()
        {
            Debug.WriteLine(resultGrammar);

            AssertGrammarStartIsPreserved();
            AssertInlinedSymbolsDoNotHaveProductions();
        }

        [Test]
        public void EpsilonIsInlined()
        {
            GivenInlinePatterns(new Symbol[0]);

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, suffix });
        }

        [Test]
        public void UnaryTerminalIsInlined()
        {
            GivenInlinePatterns(new [] { term1 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term1, suffix });
        }

        [Test]
        public void UnaryMultiTerminalsAreInlined()
        {
            GivenInlinePatterns(new [] { term1, term2, term3 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term1, term2, term3, suffix });
        }

        [Test]
        public void NestedEpsilonIsInlined()
        {
            GivenInlinePatterns(new [] { nestedNonTerm });
            originalGrammar.Productions.Add(nestedNonTerm, new Symbol[0]);

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, suffix });
        }

        [Test]
        public void NestedUnaryIsInlined()
        {
            GivenInlinePatterns(new Symbol[] { nestedNonTerm });
            originalGrammar.Productions.Add(nestedNonTerm, new[] { term4 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term4, suffix });
        }

        [Test]
        public void NestedMultitokenIsInlined()
        {
            GivenInlinePatterns(new [] { nestedNonTerm });
            originalGrammar.Productions.Add(nestedNonTerm, new[] { term4, term5 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term4, term5, suffix });
        }

        private void WhenGrammarIsInlined()
        {
            IProductionInliner target = new ProductionInliner(originalGrammar);
            this.resultGrammar = target.Inline();
        }

        private void GivenInlinePatterns(params Symbol[][] inlinePatterns)
        {
            foreach (var pattern in inlinePatterns)
            {
                originalGrammar.Productions.Add(inlinedNonTerm, pattern);
            }
        }

        private void AssertInlinedSymbolsDoNotHaveProductions()
        {
            Assert.AreEqual(
                0,
                resultGrammar.Symbols[inlinedNonTerm.Index].Productions.Count,
                "Inlined productions should be removed.");
        }

        private void AssertFlattenedProductionsPatternsAre(params Symbol[][] expectedFlattenedPatterns)
        {
            Assert.AreEqual(
                expectedFlattenedPatterns,
                resultGrammar.Symbols[start.Index].Productions.Select(p => p.PatternSymbols).ToArray(),
                "Flattened patterns should have correct inlines.");
        }

        private void AssertGrammarStartIsPreserved()
        {
            Assert.AreEqual(originalGrammar.StartToken, resultGrammar.StartToken);
        }
    }
}
