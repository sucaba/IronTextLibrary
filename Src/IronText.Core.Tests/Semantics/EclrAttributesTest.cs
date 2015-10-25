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
    public class EclrAttributesTest
    {
        [Test]
        public void SimulationSmokeTest()
        {
            var grammar = new Grammar
            {
                StartName = "S",
                Productions =
                {
                    { "S",       new [] { "DclList", "StList" } },
                    { "DclList", new [] { "DclList", "Dc" } },
                    { "DclList", new [] { "Dc" } },
                    { "StList",  new [] { "StList", "St" } },
                    { "StList",  new [] { "St" } },
                    { "Dc",      new [] { "'dcl'", "id" } },
                    { "St",      new [] { "'use'", "id" } },
                },
                Matchers = 
                {
                    { "dcl" },
                    { "use" },
                    { "id", "(alpha | '_') (alnum | '_')*" },
                    { null, "blank+" }
                },
                Reports =
                {
                    new ScannerGraphReport("EclrTest_Scanner.gv"),
                    new ParserGraphReport("EclrTest_Parser.gv"),
                    new ParserStateMachineReport("EclrTest.info")
                }
            };

            var sut = new ParserSut(grammar);
            sut.Parse("dcl x dcl y use x use y");
        }

        [Test]
        public void GlobalToInheritedTest()
        {
            var grammar = new Grammar
            {
                StartName   = "S",
                Productions = { "S = " }
            };

            grammar.DefineGlobal("val");

            var sut = new ParserSut(grammar);
            object expected = "foo-bar", got = null;
            sut.ProductionHooks.Add("S = ", ctx => got = ctx.GetInherited("val"));
            sut.Parse("", new Dictionary<string,object> { { "val", expected } });

            Assert.AreEqual(expected, got);
        }

        [Test]
        public void CopyInheritedBetweenStatesTest()
        {
            var grammar = new Grammar
            {
                StartName   = "S",
                Productions = { 
                    "S = 'x' X",
                    "X = "
                },
                Matchers = {
                   { "x" }
                },
                SymbolProperties = {
                    "X.val",
                    "S.val",
                }
            };

            grammar.DefineGlobal("val");

            grammar.SemanticActions.Add(
                SemanticAction.MakeCopyOutToOut(
                    grammar.Productions.Find("S = X"),
                    grammar.SymbolProperties.Find("S", "val"),
                    grammar.SymbolProperties.Find("X", "val")));

            var sut = new ParserSut(grammar);
            object expected = "foo-bar", got = null;
            sut.ProductionHooks.Add("X = ", ctx => got = ctx.GetInherited("val"));
            sut.Parse("x", new Dictionary<string,object> { { "val", expected } });

            Assert.AreEqual(expected, got);
        }

        [Test]
        public void CopyInheritedWithinTheSameStateTest()
        {
            var grammar = new Grammar
            {
                StartName   = "S",
                Productions = { 
                    "S = X",
                    "X = "
                },
                SymbolProperties = {
                    "X.val",
                    "S.val",
                }
            };

            grammar.DefineGlobal("val");

            grammar.SemanticActions.Add(
                SemanticAction.MakeCopyOutToOut(
                    grammar.Productions.Find("S = X"),
                    grammar.SymbolProperties.Find("S", "val"),
                    grammar.SymbolProperties.Find("X", "val")));

            var sut = new ParserSut(grammar);
            object expected = "foo-bar", got = null;
            sut.ProductionHooks.Add("X = ", ctx => got = ctx.GetInherited("val"));
            sut.Parse("", new Dictionary<string,object> { { "val", expected } });

            Assert.AreEqual(expected, got);
        }

        [Test]
        public void SynthesizedTest()
        {
            var grammar = new Grammar
            {
                StartName = "S",
                Productions = { "S = 'a' X 'z'", "X = 'b'" },
                Matchers =
                {
                    { "a" },
                    { "b" },
                    { "z" },
                }
            };

            var sut = new ParserSut(grammar);
            object expected = "foo-bar", got = null;

            sut.ProductionHooks.Add(
                "X = 'b'", 
                data => data.SetSynthesized("val", expected)
            );
            sut.ProductionHooks.Add(
                "S = 'a' X 'z'",
                data => got = data.GetSynthesized(1, "val"));

            sut.Parse("abz");

            Assert.AreEqual(expected, got);
        }
    }
}

