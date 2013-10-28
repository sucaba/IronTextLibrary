using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
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
            var scanRules = new ScanRule[]
                {
                    new SkipScanRule
                    {
                        //TokenId = -1,
                        BootstrapRegexPattern = @"[ \t]+",
                    },
                    new SingleTokenScanRule
                    {
                        //TokenId = 0,
                        BootstrapRegexPattern = @"[0-9]+(?:[.][0-9]+)? | [.][0-9]+",
                        TokenType = typeof(Num)
                    },
                    new SingleTokenScanRule
                    {
                        //TokenId = 1,
                        BootstrapRegexPattern = @"[a-zA-Z:.!@#$%^&|?*/+*=\\_-][a-zA-Z:\d.!@#$%^&|?*/+*=\\_-]*",
                        TokenType = typeof(string)
                    },
                    new SingleTokenScanRule
                    {
                        //TokenId = 2,
                        BootstrapRegexPattern = @"[""](?: \\[""] | [^""])* [""]",
                        TokenType = typeof(QStr)
                    },
                    new SingleTokenScanRule
                    {
                        //TokenId = 3,
                        LiteralText    = "(",
                        BootstrapRegexPattern = @"\("
                    },
                    new SingleTokenScanRule
                    {
                        //TokenId = 4,
                        LiteralText    = ")",
                        BootstrapRegexPattern = @"\)"
                    }
                };
            for (int i = 0; i != scanRules.Length; ++i)
            {
                ((IScanRule)scanRules[i]).Index = i;
            }

            var target = new BootstrapScanner(
                " (1 (\"bar\" +))",
                ScannerDescriptor.FromScanRules(
                    GetType().Name + "_Lexer",
                    scanRules,
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
