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
                    new[] { new Msg(TOK1, input, null, new HLoc(1, 3)) },
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
                    new[] { new Msg(TOK1, "b", null, new HLoc(1, 1)) },
                    tokens);
            }
        }

        public enum ASB { Value = 1 }

        [Language]
        public class WithSentinel
        {
            public ASB Result { get; [Produce] set; }

            [Match(@"'a' zero 'b'")]
            public ASB Asb() { return ASB.Value; }
        }

        [Language]
        public class LineComment
        {
            public string Result { get; [Produce] set; }

            [Match(@"~[a]")]
            public string Asb(string text) { return text; }
        }
    }
}
