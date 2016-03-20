using IronText.Framework;
using IronText.Reflection;
using IronText.Reports;
using IronText.Runtime;
using IronText.Tests.TestUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IronText.Tests.Semantics
{
    [TestFixture]
    public class CopyingInheritedAttributeTest
    {
        private const string StartName = "S";
        private const string CheckName = "E";
        private const string CheckProduction = CheckName + ": 'e'";
        private const string InhToAttrName = "toVal"; 
        private const string InhFromAttrName = "fromVal"; 
        private static readonly object expected = "foo-bar";
        private object got = null;
        private string formulaProductionText;
        private string input = "";
        public Grammar grammar;

        [Test]
        public void between_different_states()
        {
            GivenFormulaProduction(StartName + " = 'x' 'x' 'x'");
            GivenParserInput("xxx");

            WhenParsed();

            Assert.AreEqual(expected, got);
        }

        [Test]
        public void within_the_same_state()
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
                Productions = { CheckName + " = 'e'" },
                Matchers = { "e", "x" },
                InheritedProperties =
                {
                    { StartName, InhFromAttrName },
                    { CheckName, InhToAttrName },
                }
            };
        }
        
        private void GivenFormulaProduction(string text)
        {
            this.formulaProductionText = text + " " + CheckName;
            grammar.Productions.Add(formulaProductionText);
        }

        private void WhenParsed()
        {
            GivenInhCopyFormula();
            
            grammar.BuildIndexes();

            int fromPropIdx = grammar.InheritedProperties.Find(StartName, InhFromAttrName).Index;

            var sut = new ParserSut(grammar);
            sut.ProductionHooks.Add(
                formulaProductionText,
                ctx => got = ctx.GetInherited(fromPropIdx));
            sut.Parse(
                input + "e",
                new Dictionary<int,object>
                {
                    { fromPropIdx, expected }
                });
        }

        private void GivenInhCopyFormula()
        {
            var prod = grammar.Productions
                .Find(formulaProductionText);
            prod.Semantics
                .Add(
                    new SemanticVariable(InhToAttrName, prod.Input.Length - 1),
                    new SemanticReference(InhFromAttrName));
        }

        private void GivenParserInput(string text)
        {
            this.input = text;
        }

    }
}

