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
        public void GlobalsToInjectedParamsTest()
        {
            var grammar = new Grammar
            {
                StartName   = "S",
                Productions = { "S = ?val" }
            };

            grammar.DefineGlobal("val");

            var sut = new ParserSut(grammar);
            object expected = "foo-bar", got = null;
            sut.ProductionHooks.Add("S = ?val", new Func<object, object>(id => got = id));
            sut.Parse("", new Dictionary<string,object> { { "val", expected } });

            Assert.AreEqual(expected, got);
        }

        [Test]
        public void SymbolPropertyToInjectedParamsTest()
        {
            var grammar = new Grammar
            {
                StartName = "S",
                Productions = { "S = X ?param", "X =" }
            };

            var sut = new ParserSut(grammar);
            object expected = "foo-bar", got = null;
            sut.ProductionHooks.Add("X = ", new Func<IDataContext, object>(data => { data.SetOutput("param", expected); return null; }));
            sut.ProductionHooks.Add("S = X ?param", new Func<object, object, object>((x, val) => got = val));
            sut.Parse("");

            Assert.AreEqual(expected, got);
        }
    }
}

