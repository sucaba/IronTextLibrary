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
        public void Test()
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
                }
            };

            grammar.Reports.Add(new ScannerGraphReport("EclrTest_Scanner.gv"));
            grammar.Reports.Add(new ParserGraphReport("EclrTest_Parser.gv"));

            var sut = new ParserSut(grammar);
            sut.Parse("dcl x dcl y use x use y");
        }
    }
}
