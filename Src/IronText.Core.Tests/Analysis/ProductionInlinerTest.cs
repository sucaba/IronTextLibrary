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

        private int start;
        private int prefix;
        private int suffix;
        private int inlinedNonTerm;
        private int term1;
        private int term2;
        private int term3;
        private int term4;
        private int term5;
        private int nestedNonTerm;

        [SetUp]
        public void SetUp()
        {
            this.originalGrammar = new EbnfGrammar();
            var symbols = originalGrammar.Symbols;
            this.start  = symbols.Add("start").Index;
            this.prefix = symbols.Add("prefix").Index;
            this.suffix = symbols.Add("suffix").Index;
            this.term1  = symbols.Add("term1").Index;
            this.term2  = symbols.Add("term2").Index;
            this.term3  = symbols.Add("term3").Index;
            this.term4  = symbols.Add("term4").Index;
            this.term5  = symbols.Add("term5").Index;
            this.inlinedNonTerm = symbols.Add("inlinedNonTerm").Index;
            this.nestedNonTerm = symbols.Add("nestedNonTerm").Index;

            originalGrammar.StartToken = start;

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
            GivenInlinePatterns(new int[0]);

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, suffix });
        }

        [Test]
        public void UnaryTerminalIsInlined()
        {
            GivenInlinePatterns(new int[] { term1 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term1, suffix });
        }

        [Test]
        public void UnaryMultiTerminalsAreInlined()
        {
            GivenInlinePatterns(new int[] { term1, term2, term3 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term1, term2, term3, suffix });
        }

        [Test]
        public void NestedEpsilonIsInlined()
        {
            GivenInlinePatterns(new int[] { nestedNonTerm });
            originalGrammar.Productions.Add(nestedNonTerm, new int[0]);

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, suffix });
        }

        [Test]
        public void NestedUnaryIsInlined()
        {
            GivenInlinePatterns(new int[] { nestedNonTerm });
            originalGrammar.Productions.Add(nestedNonTerm, new[] { term4 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term4, suffix });
        }

        [Test]
        public void NestedMultitokenIsInlined()
        {
            GivenInlinePatterns(new int[] { nestedNonTerm });
            originalGrammar.Productions.Add(nestedNonTerm, new[] { term4, term5 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term4, term5, suffix });
        }

        private void WhenGrammarIsInlined()
        {
            IProductionInliner target = new ProductionInliner(originalGrammar);
            this.resultGrammar = target.Inline();
        }

        private void GivenInlinePatterns(params int[][] inlinePatterns)
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
                resultGrammar.Symbols[inlinedNonTerm].Productions.Count,
                "Inlined productions should be removed.");
        }

        private void AssertFlattenedProductionsPatternsAre(params int[][] expectedFlattenedPatterns)
        {
            Assert.AreEqual(
                expectedFlattenedPatterns,
                resultGrammar.Symbols[start].Productions.Select(p => p.Pattern).ToArray(),
                "Flattened patterns should have correct inlines.");
        }

        private void AssertGrammarStartIsPreserved()
        {
            Assert.AreEqual(originalGrammar.StartToken, resultGrammar.StartToken);
        }
    }
}
