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
    public class CopyingConstantToInheritedTest
    {
        private const string StartName = "S";
        private const string CheckName = "E";
        private const string CheckProduction = CheckName + ": 'e'";
        private const string InhToAttrName = "toVal"; 
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
                Productions = { CheckName + " = 'e'" },
                Matchers = { "e", "x" },
                InheritedProperties =
                {
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
            GivenConstantToInheritedCopyFormula();
            
            grammar.BuildIndexes();

            int toPropIdx = grammar.InheritedProperties.Find(CheckName, InhToAttrName).Index;

            var sut = new ParserSut(grammar);
            sut.ProductionHooks.Add(
                CheckProduction,
                ctx => got = ctx.GetInherited(toPropIdx));

            sut.Parse(input + "e");
        }

        private void GivenConstantToInheritedCopyFormula()
        {
            var prod = grammar.Productions
                .Find(formulaProductionText);
            prod.Semantics
                .Add(
                    new SemanticVariable(InhToAttrName, prod.Input.Length - 1),
                    new SemanticConstant(expected));
        }

        private void GivenParserInput(string text)
        {
            this.input = text;
        }

    }
}
