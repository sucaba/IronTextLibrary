using IronText.Reflection;
using NUnit.Framework;
using System.Diagnostics;
using System.Linq;

namespace IronText.Tests.Reflection.Transformations
{
    [TestFixture]
    public class ProductionInlinerTest
    {
        private Grammar originalGrammar;
        private Grammar resultGrammar;

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
            this.originalGrammar = new Grammar();
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

            originalGrammar.Productions.Define(start,  new[] { prefix, inlinedNonTerm, suffix });
            //? productionBeingExtended.Actions.Add(new ForeignAction(0, 3));
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

            /* ?
            AssertInlinedActionContainsSimpleActions(
                new ForeignAction(1, 0),
                new ForeignAction(0, 3));
            */
        }

        [Test]
        public void UnaryTerminalIsInlined()
        {
            GivenInlinePatterns(new [] { term1 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term1, suffix });

            /*?
            AssertInlinedActionContainsSimpleActions(
                new ForeignAction(1, 1),
                new ForeignAction(0, 3));
            */
        }

        [Test]
        public void UnaryMultiTerminalsAreInlined()
        {
            GivenInlinePatterns(new [] { term1, term2, term3 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term1, term2, term3, suffix });


            /*
            AssertInlinedActionContainsSimpleActions(
                new ForeignAction(1, 3),
                new ForeignAction(0, 3));
            */
        }

        [Test]
        public void NestedEpsilonIsInlined()
        {
            GivenInlinePatterns(new [] { nestedNonTerm });
            GivenNestedInlinePatterns(new Symbol[0]);

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, suffix });

            /*?
            AssertInlinedActionContainsSimpleActions(
                new ForeignAction(1, 0),
                new ForeignAction(1, 1),
                new ForeignAction(0, 3));
            */
        }

        [Test]
        public void NestedUnaryIsInlined()
        {
            GivenInlinePatterns(new Symbol[] { nestedNonTerm });
            GivenNestedInlinePatterns(new[] { term4 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term4, suffix });

            /*?
            AssertInlinedActionContainsSimpleActions(
                new ForeignAction(1, 1),
                new ForeignAction(1, 1),
                new ForeignAction(0, 3));
            */
        }

        [Test]
        public void NestedMultitokenIsInlined()
        {
            GivenInlinePatterns(new [] { nestedNonTerm });
            GivenNestedInlinePatterns(new [] { term4, term5 });

            WhenGrammarIsInlined();

            AssertFlattenedProductionsPatternsAre(new[] { prefix, term4, term5, suffix });

            /*?
            AssertInlinedActionContainsSimpleActions(
                new ForeignAction(1, 2),
                new ForeignAction(1, 1),
                new ForeignAction(0, 3));
            */
        }

        private void WhenGrammarIsInlined()
        {
            originalGrammar.Inline();
            resultGrammar = originalGrammar;
        }

        private void GivenNestedInlinePatterns(params Symbol[][] nestedInlinePatterns)
        {
            foreach (var pattern in nestedInlinePatterns)
            {
                var prod = originalGrammar.Productions.Define(nestedNonTerm, pattern);
                //? prod.Actions.Add(new ForeignAction(pattern.Length));
            }
        }

        private void GivenInlinePatterns(params Symbol[][] inlinePatterns)
        {
            foreach (var pattern in inlinePatterns)
            {
                var prod = originalGrammar.Productions.Define(inlinedNonTerm, pattern);
                //? prod.Actions.Add(new ForeignAction(pattern.Length));
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
                resultGrammar.Start.Productions.Select(p => p.Pattern).ToArray(),
                "Flattened patterns should have correct inlines.");
        }

#if false
        private void AssertInlinedActionContainsSimpleActions(
            params ForeignAction[] productionActions)
        {
            var prod = resultGrammar.Symbols[start.Index].Productions.Single();
            throw new NotImplementedException();
            ProductionAction gotAction = prod.Actions[0];
            Assert.IsInstanceOf<CompositeProductionAction>(gotAction);
            var composite = (CompositeProductionAction)gotAction;

            int count = productionActions.Length;
            Assert.AreEqual(count, composite.Subactions.Count);

            for (int i = 0; i != count; ++i)
            {
                string context = "#" + i;

                var expectedAction = productionActions[i];
                var gotSubaction   = composite.Subactions[i]; 

                Assert.AreEqual(expectedAction.Offset, gotSubaction.Offset, context);
                Assert.AreEqual(expectedAction.ArgumentCount, gotSubaction.ArgumentCount, context);
            }
        }
#endif

        private void AssertGrammarStartIsPreserved()
        {
            Assert.IsNotNull(resultGrammar.Start);
            Assert.AreEqual(originalGrammar.Start.Index, resultGrammar.Start.Index);
        }
    }
}
