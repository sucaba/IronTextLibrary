using IronText.Extensibility;
using IronText.Framework;
using IronText.Framework.Reflection;
using IronText.Lib.Ctem;
using IronText.MetadataCompiler;
using IronText.Tests.TestUtils;
using NUnit.Framework;
using System.Linq;

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

            var grammar = new EbnfGrammar
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

                ScanConditions =
                {
                    new ScanCondition("main")
                    {
                        ScanProductions = 
                        {
                            new ScanProduction(
                                    @"digit+ ('.' digit+)?  | '.' digit+", 
                                    num),
                            new ScanProduction(
                                    @"[01234567]+ 'Q'",
                                    num),
                            new ScanProduction(
                                    @"alpha alnum*",
                                    ident),
                            new ScanProduction(
                                    @"
                                    quot 
                                        ~(quot | esc)*
                                        (esc . ~(quot | esc)* )*
                                    quot
                                    ",
                                    qStr),
                            new ScanProduction(
                                    ScanPattern.CreateLiteral("begin"),
                                    begin),
                            new ScanProduction(
                                    ScanPattern.CreateLiteral("end"),
                                    end),
                            new ScanProduction(
                                    ScanPattern.CreateLiteral(":="),
                                    assign),
                            new ScanProduction(
                                    @"blank+"),
                        }
                    }
                }
            };
            
            var target = new TdfaSimulationLexer(
                "b:=10Q \"foo\"",
                ScannerDescriptor.FromScanRules(
                    GetType().Name + "_Lexer",
                    grammar.ScanConditions[0].ScanProductions,
                    ExceptionLogging.Instance));

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
