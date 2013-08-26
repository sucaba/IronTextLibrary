using System;
using System.Linq;
using IronText.Lib.Sre;
using NUnit.Framework;

namespace IronText.Tests.Lib.NfaVM
{
    /// <summary>
    /// Base test class for all SRE backends
    /// </summary>
    public abstract class SreTestBase
    {
        [Theory]
        public void EscapedTextMatch(NonEscapedText sample)
        {
            string pattern = SreSyntax.Escape(sample.Text);
            Assert.IsTrue(Match(pattern, sample.Text));
            Assert.IsFalse(Match(pattern, sample.Text.Substring(1)));
            Assert.IsFalse(Match(pattern, sample.Text.Substring(0, sample.Text.Length - 1)));
            Assert.IsFalse(Match(pattern, sample.Text + "x"));
            Assert.IsFalse(Match(pattern, "x" + sample.Text));
        }

        [Test]
        public void StandardFeaturesTest()
        {
            Assert.IsTrue(Match("\"\"", ""));
            Assert.IsTrue(Match(@"#\b", "b"));
            Assert.IsFalse(Match(@"#\b", "a"));
            Assert.IsFalse(Match(@"#\b", ""));

            Assert.IsTrue(Match(@"(? #\a)", "a"));
            Assert.IsTrue(Match(@"(? #\a)", ""));
            Assert.IsFalse(Match(@"(? #\a)", "b"));

            Assert.IsTrue(Match(@"#\a (? #\b) #\c", "abc"));
            Assert.IsTrue(Match(@"#\a (? #\b) #\c", "ac"));
            Assert.IsFalse(Match(@"#\a (? #\b) #\c", "ab"));
            Assert.IsFalse(Match(@"#\a (? #\b) #\c", "abbc"));

            Assert.IsTrue(Match(@"(or #\a #\b)", "a"));
            Assert.IsTrue(Match(@"(or #\a #\b)", "b"));
            Assert.IsFalse(Match(@"(or #\a #\b)", "c"));
            Assert.IsFalse(Match(@"(or #\a #\b)", ""));

            Assert.IsTrue(Match(@"(""ab"")", "a"));
            Assert.IsTrue(Match(@"(""ab"")", "b"));
            Assert.IsFalse(Match(@"(""ab"")", "c"));
            Assert.IsFalse(Match(@"(""ab"")", ""));

            Assert.IsTrue(Match(@"""ab""", "ab"));
            Assert.IsFalse(Match(@"""ab""", "a"));
            Assert.IsFalse(Match(@"""ab""", "abc"));
            Assert.IsFalse(Match(@"""ab""", "axb"));
            Assert.IsFalse(Match(@"""ab""", ""));

            Assert.IsTrue(Match(@"(: #\a #\b)", "ab"));
            Assert.IsFalse(Match(@"(: #\a #\b)", "a"));
            Assert.IsFalse(Match(@"(: #\a #\b)", "abc"));
            Assert.IsFalse(Match(@"(: #\a #\b)", ""));

            Assert.IsTrue(Match(@"(* #\a #\b)", ""));

            Assert.IsTrue(Match(@"(** 3 5 #\a)", "aaa"));
            Assert.IsTrue(Match(@"(** 3 5 #\a)", "aaaa"));
            Assert.IsTrue(Match(@"(** 3 5 #\a)", "aaaaa"));
            Assert.IsFalse(Match(@"(** 3 5 #\a)", "aa"));
            Assert.IsFalse(Match(@"(** 3 5 #\a)", "aaaaaa"));

            Assert.IsTrue(Match(@"(* (""ab""))", "aaabbbaabb"));
            Assert.IsFalse(Match(@"(* (""ab""))", "abc"));
            Assert.IsFalse(Match(@"(* (""ab""))", "c"));

            Assert.IsTrue(Match(@"(: (* (""ab"")) ""bb"")", "bababababbaabb"));
            Assert.IsTrue(Match(@"(: (* (""ab"")) ""bb"")", "bb"));
            Assert.IsTrue(Match(@"(: (* (""ab"")) ""bb"")", "bbbb"));
            Assert.IsTrue(Match(@"(: (* (""ab"")) ""bb"")", "abb"));
            Assert.IsFalse(Match(@"(: (* (""ab"")) ""bb"")", "bbaa"));
        }

        [Test]
        public void CharsetTest()
        {
            Assert.IsTrue(Match("(~ (\"ba\"))", "c"));
            Assert.IsFalse(Match("(~ (\"ba\"))", "a"));
            Assert.IsFalse(Match("(~ (\"ba\"))", "b"));

            Assert.IsTrue(Match("(- (\"bca\") (\"ba\"))", "c"));
            Assert.IsFalse(Match("(- (\"bca\") (\"ba\"))", "a"));
            Assert.IsFalse(Match("(- (\"bca\") (\"ba\"))", "b"));

            Assert.IsTrue(Match("(& (\"bca\") (\"dce\"))", "c"));
            Assert.IsFalse(Match("(& (\"bca\") (\"dce\"))", "b"));
            Assert.IsFalse(Match("(& (\"bca\") (\"dce\"))", "a"));
            Assert.IsFalse(Match("(& (\"bca\") (\"dce\"))", "d"));
            Assert.IsFalse(Match("(& (\"bca\") (\"dce\"))", "e"));

            string pattern = "(/ #\\a \"cdxz\" \"18\")";
            Assert.IsTrue(Match(pattern, "a"));
            Assert.IsTrue(Match(pattern, "c"));
            Assert.IsTrue(Match(pattern, "d"));
            Assert.IsTrue(Match(pattern, "x"));
            Assert.IsTrue(Match(pattern, "y"));
            Assert.IsTrue(Match(pattern, "z"));
            Assert.IsTrue(Match(pattern, "c"));
            Assert.IsTrue(Match(pattern, "1"));
            Assert.IsTrue(Match(pattern, "8"));

            Assert.IsFalse(Match(pattern, "b"));
            Assert.IsFalse(Match(pattern, "e"));
            Assert.IsFalse(Match(pattern, "0"));
            Assert.IsFalse(Match(pattern, "9"));

            Assert.IsTrue(Match("any", "a"));
            Assert.IsTrue(Match("any", "Z"));
            Assert.IsTrue(Match("any", "9"));
            Assert.IsTrue(Match("any", "?"));

            Assert.IsTrue(Match("digit", "0"));
            Assert.IsTrue(Match("digit", "9"));
            Assert.IsFalse(Match("digit", "a"));
            Assert.IsFalse(Match("digit", "Z"));
            Assert.IsFalse(Match("digit", "?"));

            Assert.IsTrue(Match("hex", "0"));
            Assert.IsTrue(Match("hex", "9"));
            Assert.IsTrue(Match("hex", "a"));
            Assert.IsTrue(Match("hex", "F"));
            Assert.IsFalse(Match("hex", "Z"));
            Assert.IsFalse(Match("hex", "?"));
        }

        [Test]
        public void LookaheadOperatorTest()
        {
            string pattern = @"""IF"" (* space) #\( (* any) #\) (* space) (look-ahead alpha)";
            Assert.IsTrue(Match(pattern, "IF (cond) T"));
            Assert.IsFalse(Match(pattern, "IF (3,4) ="));
        }

        [Test]
        public void ComplementTest()
        {
            string pattern = @"(: #\"" (* (~ #\"")) #\"")";
            Assert.IsTrue(Match(pattern, "\"foo bar\""));
            Assert.IsTrue(Match(pattern, "\"x\""));
            Assert.IsTrue(Match(pattern, "\"\""));

            Assert.IsFalse(Match(pattern, "\"\"\""));
            Assert.IsFalse(Match(pattern, "\"foo bar"));
        }

        [Test]
        public void InstanceReuseTest()
        {
            var sre = MakeSRegex(@"(or #\a #\b)");
            Assert.IsTrue(sre.Match("a"));
            Assert.IsTrue(sre.Match("b"));
            Assert.IsFalse(sre.Match("c"));
        }

        [Test]
        public void Benchmark()
        {
            var bench = new SreBenchmark(GetType().Name);
            bench.RunBenchmark(this.MakeSRegex);
            Console.WriteLine(bench.ToString());
        }


        [Test]
        public void PerformanceTest()
        {
            const int N = 25;

            string pattern = "(:" + string.Join(" ", Enumerable.Repeat(@"(? #\a)", N)) + string.Join(" ", Enumerable.Repeat(@"#\a", N)) + ")";
            Assert.IsTrue(Match(pattern, new string('a', 2 * N)));
        }

        private bool Match(string pattern, string input)
        {
            SRegex sre = MakeSRegex(pattern);
            return sre.Match(input);
        }

        protected abstract SRegex MakeSRegex(string pattern);

        public struct NonEscapedText 
        {
            public NonEscapedText(string text) { Text = text; }

            public readonly string Text;

            public override string ToString()
            {
                return string.Format("\"{0}\"", Text);
            }
        }

        [Datapoints]
        public NonEscapedText[] NonEscapedSamples = {
            new NonEscapedText(@"foo"),
            new NonEscapedText(@"f"),
            new NonEscapedText(@"#"),
            new NonEscapedText(@"\"),
            new NonEscapedText(@"?"),
            new NonEscapedText(@"*"),
            new NonEscapedText(@"|"),
            new NonEscapedText(@"/"),
            new NonEscapedText(@"&"),
            new NonEscapedText(@"#\a"),
            new NonEscapedText(@"#\return"),
            new NonEscapedText(@"#\linefeed"),
            new NonEscapedText(@"""foo"""),
            new NonEscapedText(@"(or #\a ""foo"")"),
            new NonEscapedText(@"(* #\a ""foo"")"),
            new NonEscapedText(@"(+ #\a ""foo"")"),
            new NonEscapedText(@"(? #\a ""foo"")"),
            new NonEscapedText(@"(= 123 #\a ""foo"")"),
            new NonEscapedText(@"(>= 123 #\a ""foo"")"),
            new NonEscapedText(@"(** 123 456 #\a ""foo"")"),
            new NonEscapedText(@"(or #\a ""foo"")"),
            new NonEscapedText(@"(| #\a ""foo"")"),
            new NonEscapedText(@"(: #\a ""foo"")"),
            new NonEscapedText(@"(seq #\a ""foo"")"),
            new NonEscapedText(@"(""ab"")"),
            new NonEscapedText(@"(~ #\a #\b)"),
            new NonEscapedText(@"(& #\a #\b)"),
            new NonEscapedText(@"(- #\a #\b)"),
            new NonEscapedText(@"(/ #\a ""09ix"")"),
        };
    }
}
