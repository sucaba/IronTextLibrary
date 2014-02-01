using System.Linq;
using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Samples
{
    [TestFixture]
    public class Re2cSampleTest
    {
        [Test]
        public void Test()
        {
            var lang = Language.Get(typeof(Re2cSample));
            using (var interp = new Interpreter<Re2cSample>())
            {
                var tokens = interp.Scan("print x 123 0x434af").ToArray();

                var expected = new object[] { "$print$", "x", new Decimal("123"), new HexDecimal("0x434af") };
                var got      = tokens.Select(msg => msg.Value).ToArray();
                Assert.AreEqual(expected, got);

                Assert.AreEqual(
                    new int[] { 
                        lang.Identify("print") ,
                        lang.Identify(typeof(string)),
                        lang.Identify(typeof(Decimal)),
                        lang.Identify(typeof(HexDecimal))
                    },
                    tokens.Select(msg => msg.Id));
            }
        }

        [Language]
        [UseToken("print")]
        [UseToken(typeof(string))]
        [UseToken(typeof(Decimal))]
        [UseToken(typeof(HexDecimal))]
        [ScannerGraph("Re2cSample.gv")]
        public class Re2cSample
        {
            public object Result { get; [Produce] set; }

            [Literal("print")]
            public object Keyword(string text) { return "$" + text + "$"; }

            [Match("alpha+")]
            public string Identifier(string text) { return text; }

            [Match("digit+")]
            public Decimal Decimal(string text) { return new Decimal(text); }

            [Match("'0x' hex+")]
            public HexDecimal HexDecimal(string text) { return new HexDecimal(text); }

            [Match("blank+")]
            public void Space() {}
        }

        public struct Decimal 
        {
            private   string text;
            public    Decimal(string text) { this.text = text; }
        }

        public struct HexDecimal
        {
            private   string text;
            public    HexDecimal(string text) { this.text = text; }
        }
    }
}
