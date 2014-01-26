using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Framework.Reflection;
using IronText.Lib.Ctem;
using IronText.MetadataCompiler;
using IronText.Tests.TestUtils;
using NUnit.Framework;

namespace IronText.Tests.Bootstrap
{
    [TestFixture]
    public class BootstrapLexerTest
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
                Symbols = { 
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
                                    ScanPattern.CreateRegular(
                                        null,
                                        @"[ \t]+"))
                            {
                                Joint = { new CilScanProduction(typeof(void)) }
                            },
                            new ScanProduction(
                                    ScanPattern.CreateRegular(
                                        null,
                                        @"[0-9]+(?:[.][0-9]+)? | [.][0-9]+"), 
                                    num)
                            {
                                Joint = {  new CilScanProduction(typeof(Num)) }
                            },
                            new ScanProduction(
                                    ScanPattern.CreateRegular(
                                        null,
                                        @"[a-zA-Z:.!@#$%^&|?*/+*=\\_-][a-zA-Z:\d.!@#$%^&|?*/+*=\\_-]*"),
                                    ident)
                            {
                                Joint = { new CilScanProduction(typeof(string)) }
                            },
                            new ScanProduction(
                                    ScanPattern.CreateRegular(
                                        null,
                                        @"[""](?: \\[""] | [^""])* [""]"),
                                    qStr)
                            {
                                Joint = { new CilScanProduction(typeof(QStr)) }
                            },
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

            var target = new BootstrapScanner(
                " (1 (\"bar\" +))",
                ScannerDescriptor.FromScanRules(
                    GetType().Name + "_Lexer",
                    grammar.ScanConditions[0].ScanProductions,
                    ExceptionLogging.Instance),
                null,
                new StubTokenRefResolver(),
                ExceptionLogging.Instance);

            var collector = new Collector<Msg>();
            target.Accept(collector);
            Assert.AreEqual(
                new object[] { null, new Num("1"), null, new QStr("bar"), "+", null, null },
                collector.Select(msg => msg.Value).ToArray());
        }
    }
}
