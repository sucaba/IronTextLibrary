using System.Linq;
using IronText.Extensibility;
using IronText.Lib.Ctem;
using IronText.Logging;
using IronText.MetadataCompiler;
using IronText.Runtime;
using IronText.Tests.TestUtils;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class DfaSimulationLexerTest
    {
        [Test]
        public void Test()
        {
            var scanRules = new ScanRule[]
                {
                    new SkipScanRule
                    {
                        Pattern = @"blank+",
                    },
                    new SingleTokenScanRule
                    {
                        Pattern = @"digit+ ('.' digit+)?  | '.' digit+",
                        TokenType = typeof(Num)
                    },
                    new SingleTokenScanRule
                    {
                        Pattern = @"(alpha | [:.!@#$%^&|?*/+*=\\_-])
                                       (alnum | [:.!@#$%^&|?*/+*=\\_-])*",
                        TokenType = typeof(string)
                    },
                    new SingleTokenScanRule
                    {
                        Pattern = "'\"' ('\\\\\"' | ~'\"')* '\"'",
                        TokenType = typeof(QStr)
                    },
                    new SingleTokenScanRule
                    {
                        LiteralText    = "(",
                        Pattern = @"'('",
                    },
                    new SingleTokenScanRule
                    {
                        LiteralText    = ")",
                        Pattern = @"')'"
                    }
                };
            
            var target = new DfaSimulationLexer(
                " (1 (\"bar\" +))",
                ScannerDescriptor.FromScanRules(
                    GetType().Name + "_Lexer",
                    scanRules,
                    ExceptionLogging.Instance));

            var collector = new Collector<Msg>();
            target.Accept(collector);
            Assert.AreEqual(
                new object[] { null, new Num("1"), null, new QStr("bar"), "+", null, null },
                collector.Select(msg => msg.Value).ToArray());
        }
    }
}
