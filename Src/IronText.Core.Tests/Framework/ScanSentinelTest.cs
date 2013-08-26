using System.Linq;
using IronText.Framework;
using IronText.Logging;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class ScanSentinelTest
    {
        [Test]
        public void ExplicitSentinelCharTest()
        {
            var lang = Language.Get(typeof(WithSentinel));
            int TOK1 = lang.Identify(typeof(ASB));
            var input = "a\0b";
            using (var interp = new Interpreter<WithSentinel>())
            {
                var tokens = interp.Scan(input).ToArray();

                Assert.AreEqual(
                    new[] { new Msg(TOK1, ASB.Value, new Loc(0, 3)) },
                    tokens);
            }
        }

        [Test]
        public void ImplicitSentinelTest()
        {
            var lang = Language.Get(typeof(LineComment));
            int TOK1 = lang.Identify(typeof(string));

            using (var interp = new Interpreter<LineComment>())
            {
                var input = "b";
                var tokens = interp.Scan(input).ToArray();

                Assert.AreEqual(
                    new[] { new Msg(TOK1, "b", new Loc(0, 1)) },
                    tokens);
            }
        }

        public enum ASB { Value = 1 }

        [Language]
        [UseToken(typeof(ASB))]
        public class WithSentinel
        {
            public WithSentinel Result { get; [Parse] set; }

            [Scan(@"'a' zero 'b'")]
            public ASB Asb() { return ASB.Value; }
        }

        [Language]
        public class LineComment
        {
            public string Result { get; [Parse] set; }

            [Scan(@"~[a]")]
            public string Asb(string text) { return text; }
        }
    }
}
