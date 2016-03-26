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
#if false
    [TestFixture]
    public class CopyingSynthesizedToInheritedAttributeTest
    {
        private const string StartName = "S";
        private const string CheckName = "E";
        private const string SynthName = "Synth";
        private const string GlobalInhName = "GlobalInh";
        private const string InhName = "Inh";
        private const string CheckProduction = CheckName + ": 'e'";
        private static readonly object expected = "foo-bar";
        private object got = null;
        private string formulaProductionText;
        private string input = "";
        public Grammar grammar;

        [Test]
        public void between_different_states()
        {
            GivenFormulaProduction(StartName + " = F 'x' 'x' 'x'");
            GivenSynthesizedProduction("F = 'f'");
            GivenParserInput("xxx");

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
                    { StartName, GlobalInhName }
                }
            };
        }
        
        private void GivenFormulaProduction(string text)
        {
            this.formulaProductionText = text + " " + CheckName;
            var formulaProd = grammar.Productions.Add(formulaProductionText);

            ProvideInheritedValueToBeUsedInSynthProductionFormula(formulaProd);
        }

        private static void ProvideInheritedValueToBeUsedInSynthProductionFormula(Production formulaProd)
        {
            formulaProd.Semantics.Add(
                new SemanticVariable(InhName, 0),
                new SemanticReference(GlobalInhName));
        }

        private void GivenSynthesizedProduction(string prodText)
        {
            var prod = grammar.Productions.Add(prodText);

            prod.Semantics.Add(
                new SemanticVariable(SynthName),
                new SemanticReference(InhName));
        }

        private void WhenParsed()
        {
            GivenInhCopyFormula();
            
            grammar.BuildIndexes();

            int globalPropIdx = grammar.InheritedProperties.Find(StartName, GlobalInhName).Index;
            int inhPropIdx = grammar.InheritedProperties.Find(StartName, InhName).Index;

            var sut = new ParserSut(grammar);
            sut.ProductionHooks.Add(
                CheckProduction,
                ctx => got = ctx.GetInherited(inhPropIdx));
            sut.Parse(
                input + "e",
                new Dictionary<int,object>
                {
                    { globalPropIdx, expected }
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
#endif
}

