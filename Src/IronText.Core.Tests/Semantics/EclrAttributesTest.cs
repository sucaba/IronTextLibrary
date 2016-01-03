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
    [Explicit]
    public class EclrAttributesTest
    {
        [Test]
        public void AttributeECsAreIdentifiedTest()
        {
            // INH Equivalence Class (EC) rules:
            // 1) Different attribute names within the same EC cannot belong to the same grammar symbol.
            //    EC stack node can contain only single value while 2 INH attributes can have different values.
            // 2) INH attributes belong to the same EC if there is at least one copy rule between them and they
            //    are not violating rule 1.
            // 3) Copy rules between attributes in the same EC will not be executed in runtime 
            //    because they are not needed.
            // 4) For implementation simplicity EC stacks are synchronized with a parsing stack.
            var grammar = new Grammar
            {
                StartName = "S",
                Productions =
                {
                    @"S : E E E " +
                        "{ E[1].Env = S.Env + 1 }" + // State push
                        "{ E[2].Env = S.Env }"     + // Reuse value from stack with offset -2 (-1 is stack top)
                        "{ E[3].Env = S.Env2 }"      // State push
                                                     // S.Env, S.Env2 are in different ECs according to the rule #1
                    ,   
                    // [ x, x + 1, y ]
                    "E : 't' { use E.Env /* EC?, offset? */ }",
                    // E.Env => EC1_stack[-1] // last
                }
            };
        }

#if ENABLE_SEM0
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
            // $start.val = "foo-bar"
            // S = X   s{ X.val = S.val }
            // X = "x" s{ assert X.val == "foo-bar" }

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
#endif
    }
}

