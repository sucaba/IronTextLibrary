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
using IronText.Reflection.Managed;

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

            var grammar = new Grammar
            {
                Symbols = { 
                    lParen,
                    rParen,
                    num,
                    ident,
                    qStr
                },

                Matchers = 
                {
                    new Matcher(
                            ScanPattern.CreateRegular(
                                null,
                                @"[ \t]+"))
                    {
                        Joint = { new CilMatcher(typeof(void)) }
                    },
                    new Matcher(
                            ScanPattern.CreateRegular(
                                null,
                                @"[0-9]+(?:[.][0-9]+)? | [.][0-9]+"), 
                            num)
                    {
                        Joint = {  new CilMatcher(typeof(Num)) }
                    },
                    new Matcher(
                            ScanPattern.CreateRegular(
                                null,
                                @"[a-zA-Z:.!@#$%^&|?*/+*=\\_-][a-zA-Z:\d.!@#$%^&|?*/+*=\\_-]*"),
                            ident)
                    {
                        Joint = { new CilMatcher(typeof(string)) }
                    },
                    new Matcher(
                            ScanPattern.CreateRegular(
                                null,
                                @"[""](?: \\[""] | [^""])* [""]"),
                            qStr)
                    {
                        Joint = { new CilMatcher(typeof(QStr)) }
                    },
                    new Matcher(
                            ScanPattern.CreateLiteral("("),
                            lParen),
                    new Matcher(
                            ScanPattern.CreateLiteral(")"),
                            rParen),
                }
            };

            var target = new BootstrapScanner(
                " (1 (\"bar\" +))",
                ScannerDescriptor.FromScanRules(grammar.Matchers, ExceptionLogging.Instance),
                null,
                ExceptionLogging.Instance);

            var collector = new Collector<Msg>();
            target.Accept(collector);
            Assert.AreEqual(
                new object[] { null, new Num("1"), null, new QStr("bar"), "+", null, null },
                collector.Select(msg => msg.Value).ToArray());
        }
    }
}
