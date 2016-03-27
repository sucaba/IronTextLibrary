using System;
using IronText.Reflection;
using IronText.Tests.TestUtils;
using NUnit.Framework;

namespace IronText.Tests.Semantics
{
    [TestFixture]
    public class CopyingSynthesizedToSynthesizedTest
    {
        private const string StartName = "S";
        private const string SynthName = "toVal";
        private const string SynthInputName = "fromVal";
        private static readonly object expected = "foo-bar";
        private object got = null;
        private string formulaProductionText;
        private string input = "";
        public Grammar grammar;

        [Test]
        public void in_non_empty_production()
        {
            GivenFormulaProduction(StartName + " = X");
            GivenSythesizedSourceProduction("X = 'x'");
            GivenParserInput("x");

            WhenParsed();

            Assert.AreEqual(expected, got);
        }

        [SetUp]
        public void SetUp()
        {
            this.grammar = new Grammar
            {
                StartName = StartName,
                Matchers = { "x" }
            };
        }
        
        private void GivenFormulaProduction(string text)
        {
            this.formulaProductionText = text;
            grammar.Productions
                .Add(formulaProductionText)
                .Semantics
                .Add(
                    new SemanticVariable(SynthName),
                    new SemanticReference(SynthInputName, 0));
        }

        private void GivenSythesizedSourceProduction(string prodText)
        {
            grammar.Productions
                .Add(prodText)
                .Semantics
                .Add(
                    new SemanticVariable(SynthInputName),
                    new SemanticConstant(expected));
        }

        private void WhenParsed()
        {
            var sut = new ParserSut(grammar);

            int toPropIdx = grammar.SymbolProperties.Find(StartName, SynthName).Index;

            sut.ProductionHooks.Add(
                formulaProductionText,
                ctx => got = ctx.GetSynthesized(toPropIdx));

            sut.Parse(input);
        }

        private void GivenParserInput(string text)
        {
            this.input = text;
        }

    }
}
