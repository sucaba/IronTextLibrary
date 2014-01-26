using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Samples
{
    [TestFixture]
    public class DragonBookSamplesTest
    {
        [Test]
        public void Test()
        {
            Parse("Foo");
            Parse("Foo = Bar");
            Parse("*Foo = Bar");
        }

        public void Parse(string text)
        {
            var context = new Lr1DragonBookSample();
            Language.Parse(context, text);
        }

        public interface S { }
        public interface L { }
        public interface R { }

        // LR(1) grammar which cannot be parsed by the SLR parser
        [Language]
        public class Lr1DragonBookSample
        {
            public S Result { get; [Parse] set; }

            [Parse(null, "=", null)]
            public S Equility(L l, R r) { return null; }

            [Parse]
            public S Ref(R r) { return null; }

            [Parse("*")]
            public L StarR(R r) { return null; }

            [Parse]
            public L Reference(string idn) { return null; }

            [Parse]
            public R Left(L l) { return null; }

            [Scan("blank+")]
            public void Space() { }

            [Scan("alpha alnum*")]
            public string Identifier() { return null; }
        }
    }
}
