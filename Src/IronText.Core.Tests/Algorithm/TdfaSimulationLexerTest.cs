using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.MetadataCompiler;
using IronText.Tests.TestUtils;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class TdfaSimulationLexerTest
    {
        [Test]
        public void Test()
        {
            var scanRules = new ScanRule[]
                {
                    new SingleTokenScanRule
                    {
                        Pattern = @"alpha alnum*",
                        TokenType = typeof(string)
                    },
                    new SingleTokenScanRule
                    {
                        Pattern = @"digit+",
                        TokenType = typeof(Num)
                    },
                    new SingleTokenScanRule
                    {
                        Pattern = @"[01234567]+ 'Q'",
                        TokenType = typeof(Num)
                    },
                    new SingleTokenScanRule
                    {
                        Pattern = @"
                                quot 
                                    ~(quot | esc)*
                                    (esc . ~(quot | esc)* )*
                                quot
                                ",
                        TokenType = typeof(QStr)
                    },
                    new SingleTokenScanRule
                    {
                        LiteralText = "begin",
                        Pattern     = @"'begin'",
                    },
                    new SingleTokenScanRule
                    {
                        LiteralText = "end",
                        Pattern     = @"'end'"
                    },
                    new SingleTokenScanRule
                    {
                        LiteralText = ":=",
                        Pattern     = @"':='"
                    },
                    new SingleTokenScanRule
                    {
                        Pattern   = @"blank+",
                        TokenType = typeof(void)
                    }
                };
            
            var target = new TdfaSimulationLexer(
                "b:=10Q \"foo\"",
                ScannerDescriptor.FromScanRules(
                    GetType().Name + "_Lexer",
                    scanRules,
                    ExceptionLogging.Instance));

            var collector = new Collector<Msg>();
            target.Accept(collector);
            Assert.AreEqual(4, collector.Count);
        }
    }
}
