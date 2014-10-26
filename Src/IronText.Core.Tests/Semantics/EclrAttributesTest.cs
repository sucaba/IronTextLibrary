using IronText.Framework;
using IronText.Reflection;
using IronText.Reports;
using IronText.Runtime;
using IronText.Tests.TestUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
                StartName = "S",
                Productions =
                {
                    { "S",      new [] { "?val" } },
                },
                /*
                Matchers = 
                {
                    { null, "blank+" }
                },
                */
                Reports =
                {
                    new ScannerGraphReport("EclrGlobalsToInjectedParamsTest_Scanner.gv"),
                    new ParserGraphReport("EclrGlobalsToInjectedParamsTest.gv"),
                    new ParserStateMachineReport("EclrGlobalsToInjectedParamsTest.info")
                }
            };

            var globalValProp = new InheritedProperty(grammar.Start, "val");
            grammar.InheritedProperties.Add(globalValProp);

            var sut = new ParserSut(grammar);
            string expectedVal = "foo-bar";
            object gotVal       = null;
            sut.ProductionHooks.Add("S = ?val", new Func<object, object>(id => gotVal = id));
            sut.Parse("", new Dictionary<string,object> { { globalValProp.Name, expectedVal } });

            Assert.AreEqual(expectedVal, gotVal);
        }
    }
}
