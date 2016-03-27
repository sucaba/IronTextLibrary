using IronText.Reflection;
using IronText.Tests.TestUtils;
using NUnit.Framework;

namespace IronText.Tests.Semantics
{
    [TestFixture]
    public class CopyingConstantToSynthesizedTest
    {
        private const string StartName = "S";
        private const string SynthName = "toVal";
        private static readonly object expected = "foo-bar";
        private object got = null;
        private string formulaProductionText;
        private string input = "";
        public Grammar grammar;

        [Test]
        public void in_the_middle_of_production()
        {
            GivenFormulaProduction(StartName + " = 'x' 'x' 'x'");
            GivenParserInput("xxx");

            WhenParsed();

            Assert.AreEqual(expected, got);
        }

        [Test]
        public void at_a_production_start()
        {
            GivenFormulaProduction(StartName + " = ");
            GivenParserInput("");

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
                    new SemanticConstant(expected));
        }

        private void WhenParsed()
        {
            grammar.BuildIndexes();

            int toPropIdx = grammar.SymbolProperties.Find(StartName, SynthName).Index;

            var sut = new ParserSut(grammar);
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
