using IronText.Reflection;
using IronText.Tests.TestUtils;
using NUnit.Framework;
using System.Collections.Generic;

namespace IronText.Tests.Semantics
{
    [TestFixture]
    public class PassingGlobalInheritedAttributeTest
    {
        private const string StartName = "S";
        private const string InhAttrName = "val"; 
        private static readonly object expected = "foo-bar";
        private object got = null;
        private string startProductionText;
        private string input = "";
        public Grammar grammar;

        [SetUp]
        public void SetUp()
        {
            CreateGrammar();
        }

        [Test]
        public void when_grammar_is_empty()
        {
            GivenStartProduction(StartName + " = ");
            GivenParserInput("");

            WhenParsed();

            Assert.AreEqual(got, expected);
        }

        
        private void GivenStartProduction(string text)
        {
            this.startProductionText = text;
            grammar.Productions.Add(startProductionText);
        }

        private void WhenParsed()
        {
            grammar.BuildIndexes();

            int fromPropIdx = grammar.InheritedProperties.Find(StartName, InhAttrName).Index;

            var sut = new ParserSut(grammar);
            sut.ProductionHooks.Add(
                startProductionText,
                ctx => got = ctx.GetInherited(fromPropIdx));
            sut.Parse(
                input,
                new Dictionary<int,object>
                {
                    { fromPropIdx, expected }
                });
        }

        private void GivenParserInput(string text)
        {
            this.input = text;
        }

        private void CreateGrammar()
        {
            this.grammar = new Grammar
            {
                StartName = StartName,
                Matchers = { "e", "x" },
                InheritedProperties =
                {
                    { StartName, InhAttrName }
                }
            };
        }
    }
}

