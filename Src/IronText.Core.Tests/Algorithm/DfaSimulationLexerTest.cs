using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection;
using IronText.Lib.Ctem;
using IronText.MetadataCompiler;
using IronText.Tests.TestUtils;
using NUnit.Framework;
using IronText.Logging;
using IronText.Runtime;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class DfaSimulationLexerTest
    {
        [Test]
        public void Test()
        {
            var lParen = new Symbol("(");
            var rParen = new Symbol(")");
            var num    = new Symbol("NUM");
            var ident  = new Symbol("ID");
            var qStr   = new Symbol("QSTR");

            var grammar = new EbnfGrammar
            {
                Symbols = 
                { 
                    lParen,
                    rParen,
                    num,
                    ident,
                    qStr
                },

                ScanConditions =
                {
                    new ScanCondition("main")
                    {
                        ScanProductions = 
                        {
                            new ScanProduction(
                                    @"blank+"),
                            new ScanProduction(
                                    @"digit+ ('.' digit+)?  | '.' digit+", 
                                    num),
                            new ScanProduction(
                                    @"(alpha | [:.!@#$%^&|?*/+*=\\_-]) (alnum | [:.!@#$%^&|?*/+*=\\_-])*",
                                    ident),
                            new ScanProduction(
                                    "'\"' ('\\\\\"' | ~'\"')* '\"'",
                                    qStr),
                            new ScanProduction(
                                    ScanPattern.CreateLiteral("("),
                                    lParen),
                            new ScanProduction(
                                    ScanPattern.CreateLiteral(")"),
                                    rParen),
                        }
                    }
                }
            };
           
            var target = new DfaSimulationLexer(
                " (1 (\"bar\" +))",
                ScannerDescriptor.FromScanRules(
                    GetType().Name + "_Lexer",
                    grammar.ScanConditions[0].ScanProductions,
                    ExceptionLogging.Instance));

            var collector = new Collector<Msg>();
            target.Accept(collector);
            Assert.AreEqual(
                new int[] { lParen.Index, num.Index, lParen.Index, qStr.Index, ident.Index, rParen.Index, rParen.Index },
                collector.Select(msg => msg.Id).ToArray());
            Assert.AreEqual(
                new object[] { "(", "1", "(", "\"bar\"", "+", ")", ")" },
                collector.Select(msg => msg.Value).ToArray());
        }
    }
}
