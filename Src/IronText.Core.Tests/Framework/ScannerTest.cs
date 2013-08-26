using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Framework;
using IronText.Lib;
using IronText.Lib.Ctem;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Syntax.Re2IL
{
    [TestFixture]
    public class ScannerTest
    {
        [Test]
        public void LanguageIsValid()
        {
            var lang = Language.Get(typeof(MyMiniLexer));

            var whileKwdId = lang.Identify("while");
            Assert.IsTrue(whileKwdId >= 0);

            var ifKwdId = lang.Identify("if");
            Assert.IsTrue(ifKwdId == -1);

            Assert.AreEqual(lang.Identify(typeof(WhileKwd)), lang.Identify("while")); // literal and type
            Assert.AreEqual(-1, lang.Identify("if"));       // not literal but scan pattern
        }

        [Test]
        public void TestScanner()
        {
            var lang = Language.Get(typeof(MyMiniLexer));
            int ID  = lang.Identify(typeof(string));
            int OPN = lang.Identify("(");
            int CLS = lang.Identify(")");
            int NUM = lang.Identify(typeof(Num));
            int QSTR = lang.Identify(typeof(QStr));
            int WHILE = lang.Identify(typeof(WhileKwd));
            int IF = lang.Identify(typeof(IfKwd));
            int SPECSYM = lang.Identify(typeof(SpecSymb));

            var tokenSample = new Dictionary<int, string>
            {
                { ID,    "foo"},
                { OPN,   "(" },
                { CLS,   ")" },
                { NUM,   "123" },
                { QSTR,  "\"bar\"" },
                { WHILE, "while" },
                { IF,    "if" },
                { SPECSYM, "%" },
            };

            var tokens = new int[] {
                        OPN,
                        CLS,
                        NUM,
                        QSTR,
                        ID,
                        WHILE,
                        IF,
                        SPECSYM
            };

            AssertScanned(";", new int[0]);
            AssertScanned(";c", new int[0]);
            AssertScanned("; foo bar", new int[0]);

            // Single token parsing
            foreach (var token in tokens)
            {
                AssertScanned(tokenSample[token], new [] { token });
            }

            Predicate<int> IsKeyword = t => t == IF || t == WHILE;

            // Pairwise tests
            foreach (var leftToken in tokens)
                foreach (var rightToken in tokens)
                {
                    bool needSeparator = (leftToken == ID || leftToken == NUM || IsKeyword(leftToken))
                                      && (rightToken == ID || rightToken == NUM || IsKeyword(rightToken));
                    string separator = needSeparator ? " " : "";
                    AssertScanned(
                        tokenSample[leftToken] + separator + tokenSample[rightToken],
                        new [] { leftToken, rightToken });
                }

            // Test errors:
            string[] invalidInputs = { ",", " ,", "+", "1", "1 ", " 1" };
            foreach (var input in invalidInputs)
            {
                Assert.Throws(
                    Is.InstanceOf<Exception>(),
                    () => Scan(input),
                    "Input was incorrectly accepted: \"" + input + "\"");
            }

            AssertScanned(
                " (12 (\"bar\" foo while loop)) % $",
                new [] 
                { 
                    OPN,
                    NUM,
                    OPN,
                    QSTR,
                    ID,
                    WHILE,
                    WHILE,
                    CLS,
                    CLS,
                    SPECSYM,
                    SPECSYM
                });
        }

        private static void AssertScanned(string input, int[] expectedTokens)
        {
            var tokens = Scan(input);

            Assert.AreEqual(
                expectedTokens,
                tokens.Select(msg => msg.Id).ToArray());
        }

        private static List<Msg> Scan(string input)
        {
            var context = new MyMiniLexer();
            using (var interp = new Interpreter<MyMiniLexer>())
            {
                var result = interp.Scan(input).ToList();
                return result;
            }
        }

        [Language] // Start type is not important because only scanner is used
        [ScannerGraph("MyMiniLexer.gv")]
        [ScannerDocument("MyMiniLexer.scan")]
        [StaticContext(typeof(Builtins))]
        [UseToken(typeof(Num))]
        [UseToken(typeof(QStr))]
        [UseToken(typeof(string))]
        [UseToken(typeof(IfKwd))]
        [UseToken(typeof(WhileKwd))]
        [UseToken(typeof(SpecSymb))]
        [UseToken("(")]
        [UseToken(")")]
        public class MyMiniLexer
        {
            public MyMiniLexer Result { get; [Parse] set; }

            [Scan("blank+")]
            public void WhiteSpace() { }

            [Scan(@"';' ~[\r\n]*")]
            public void LineComment() { }

            [Scan("digit digit+")]
            public Num Number(string token) { return new Num(token); }

            // Two literals on the same method
            [Literal("while")]
            [Literal("loop")]
            public WhileKwd WhileKeyword(string text) { return null; }
            
            // Type-token :
            [Scan("'if'")]
            public IfKwd IfKeyword(string text) { return null; }

            // Two scan methods on the same method
            [Scan("'%'")]
            [Scan("'$'")]
            public SpecSymb SpecSymb() { return null; }

            [Scan("alpha alnum*")]
            public string Identifier(string token) { return token; }

            [Scan(@"
                quot 
                ~(quot | esc)*
                (esc . ~(quot | esc)* )*
                quot")]
            public QStr Str(char[] buffer, int start, int length)
            {
                return QStr.Parse(buffer, start, length);
            }
        }

        public interface IfKwd { }
        public interface WhileKwd { }
        public interface SpecSymb { }
    }
}
