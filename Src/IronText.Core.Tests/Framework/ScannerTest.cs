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

            Assert.AreEqual(lang.Identify(typeof(WhileKwd)), lang.Identify("while")); // literal and type
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
            int GRAVEACCENTA = lang.Identify(typeof(GraveAccentA));
            int SURROGATE1 = lang.Identify(typeof(Surrogate1));

            var tokenSample = new Dictionary<int, string[]>
            {
                { ID,    new [] { "foo", "xhile", "whilee"}},
                { OPN,   new [] { "(" } },
                { CLS,   new [] { ")" } },
                { NUM,   new [] { "123" } },
                { QSTR,  new [] { "\"bar\"" } },
                { WHILE, new [] { "while" } },
                { SPECSYM, new [] { "%" } },
                { GRAVEACCENTA, new [] { "\u00C0" , "\u0060A" } },
                { SURROGATE1, new [] { "\U0010FFFF" } },
            };

            var tokens = new int[] {
                        OPN,
                        CLS,
                        NUM,
                        QSTR,
                        ID,
                        WHILE,
                        SPECSYM,
                        GRAVEACCENTA,
                        SURROGATE1
            };

            AssertScanned(";", new int[0]);
            AssertScanned(";c", new int[0]);
            AssertScanned("; foo bar", new int[0]);

            // Single token parsing
            foreach (var token in tokens)
                foreach (var sample in tokenSample[token])
                {
                    AssertScanned(sample, new [] { token });
                }

            Predicate<int> IsKeyword = t => t == WHILE;

            // Pairwise tests
            foreach (var leftToken in tokens)
            foreach (var leftSample in tokenSample[leftToken])
            foreach (var rightToken in tokens)
            foreach (var rightSample in tokenSample[rightToken])
            {
                bool needSeparator = (leftToken == ID || leftToken == NUM || IsKeyword(leftToken))
                                  && (rightToken == ID || rightToken == NUM || IsKeyword(rightToken));
                string separator = needSeparator ? " " : "";
                AssertScanned(
                    leftSample + separator + rightSample,
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
                tokens.Select(msg => msg.AmbiguousToken).ToArray());
        }

        private static List<Message> Scan(string input)
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
        [UseToken(typeof(GraveAccentA))]
        [UseToken(typeof(Surrogate1))]
        [UseToken("(")]
        [UseToken(")")]
        public class MyMiniLexer
        {
            public MyMiniLexer Result { get; [Produce] set; }

            [Match("blank+")]
            public void WhiteSpace() { }

            [Match(@"';' ~[\r\n]*")]
            public void LineComment() { }

            [Match("digit {2,}")]
            public Num Number(string token) { return new Num(token); }

            // Two literals on the same method
            [Literal("while")]
            [Literal("loop")]
            public WhileKwd WhileKeyword(string text) { return null; }
            
            // Two scan methods on the same method
            [Match("'%'")]
            [Match("'$'")]
            public SpecSymb SpecSymb() { return null; }

            // Type-token :
            [Literal("if")]
            public IfKwd IfKeyword(string text) { return null; }

            [Match("alpha alnum{0,10}")]
            public string Identifier(string token) { return token; }

            [Match(@"
                quot 
                ~(quot | esc)*
                (esc . ~(quot | esc)* )*
                quot")]
            public QStr Str(string text)
            {
                return QStr.Parse(text);
            }

            // Two ways to represent A with a grave accent on top
            [Match("u00c0")]
            [Match("u00C0")]
            [Match("u0060 'A'")]
            public GraveAccentA GraveAccent(string text) { return null; }

            // Surrogate sample
            [Match("U0010ffff")]
            [Match("U0010FFFF")]
            public Surrogate1 Surrogate1(string text) { return null; }
        }

        public interface IfKwd { }
        public interface WhileKwd { }
        public interface SpecSymb { }
        public interface Surrogate1 { }
        public interface GraveAccentA { }
    }
}
