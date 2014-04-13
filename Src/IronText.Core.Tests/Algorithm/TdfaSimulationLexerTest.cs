using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection;
using IronText.Lib.Ctem;
using IronText.MetadataCompiler;
using IronText.Tests.TestUtils;
using NUnit.Framework;
using System.Linq;
using IronText.Logging;
using IronText.Runtime;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class TdfaSimulationLexerTest
    {
        [Test]
        public void Test()
        {            
            var num    = new Symbol("NUM");
            var ident  = new Symbol("ID");
            var qStr   = new Symbol("QSTR");
            var begin  = new Symbol("begin");
            var end    = new Symbol("end");
            var assign = new Symbol(":=");

            var grammar = new Grammar
            {
                Symbols =
                { 
                    num,
                    ident,
                    qStr,
                    begin,
                    end,
                    assign
                },

                Matchers = 
                {
                    new Matcher(
                            @"digit+ ('.' digit+)?  | '.' digit+", 
                            num),
                    new Matcher(
                            @"[01234567]+ 'Q'",
                            num),
                    new Matcher(
                            @"alpha alnum*",
                            ident),
                    new Matcher(
                            @"
                            quot 
                                ~(quot | esc)*
                                (esc . ~(quot | esc)* )*
                            quot
                            ",
                            qStr),
                    new Matcher(
                            ScanPattern.CreateLiteral("begin"),
                            begin),
                    new Matcher(
                            ScanPattern.CreateLiteral("end"),
                            end),
                    new Matcher(
                            ScanPattern.CreateLiteral(":="),
                            assign),
                    new Matcher(
                            @"blank+"),
                }
            };
            
            var target = new TdfaSimulationLexer(
                "b:=10Q \"foo\"",
                ScannerDescriptor.FromScanRules(grammar.Matchers, ExceptionLogging.Instance));

            var collector = new Collector<Msg>();
            target.Accept(collector);

            Assert.AreEqual(
                new int[] { ident.Index, assign.Index, num.Index, qStr.Index },
                collector.Select(msg => msg.Id).ToArray());
            Assert.AreEqual(
                new object[] { "b", ":=", "10Q", "\"foo\"" },
                collector.Select(msg => msg.Value).ToArray());
        }
    }
}
