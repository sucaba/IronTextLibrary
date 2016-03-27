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
    public class CopyingSynthesizedToInheritedAttributeTest
    {
        private const string StartName = "S";
        private const string CheckName = "E";
        private const string SynthName = "Synth";
        private const string InhName = "Inh";
        private const string CheckProduction = CheckName + ": 'e'";
        private static readonly object expected = "foo-bar";
        private object got = null;
        private string formulaProductionText;
        private string input = "";
        public Grammar grammar;

        [Test]
        public void between_different_parse_nodes()
        {
            GivenFormulaProduction(StartName + " = F 'x' 'x' 'x'");
            GivenSynthesizedProduction("F = 'f'");
            GivenParserInput("fxxx");

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
                Matchers = { "f", "e", "x" }
            };
        }
        
        private void GivenFormulaProduction(string text)
        {
            this.formulaProductionText = text + " " + CheckName;
            grammar.Productions.Add(formulaProductionText);
        }

        private void GivenSynthesizedProduction(string prodText)
        {
            var prod = grammar.Productions.Add(prodText);

            prod.Semantics.Add(
                new SemanticVariable(SynthName),
                new SemanticConstant(expected));
        }

        private void WhenParsed()
        {
            GivenSynthToInhCopyFormula();
            
            grammar.BuildIndexes();

            int inhPropIdx = grammar.InheritedProperties.Find(CheckName, InhName).Index;

            var sut = new ParserSut(grammar);
            sut.ProductionHooks.Add(
                CheckProduction,
                ctx => got = ctx.GetInherited(inhPropIdx));
            sut.Parse(input + "e");
        }

        private void GivenSynthToInhCopyFormula()
        {
            var prod = grammar.Productions
                .Find(formulaProductionText);
            prod.Semantics
                .Add(
                    new SemanticVariable(InhName, prod.Input.Length - 1),
                    new SemanticReference(SynthName, 0));
        }

        private void GivenParserInput(string text)
        {
            this.input = text;
        }
    }
}

