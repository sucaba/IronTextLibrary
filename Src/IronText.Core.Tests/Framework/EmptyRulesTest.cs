using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class EmptyRulesTest
    {
        [Test]
        public void Test()
        {
            var result = Language.Parse(new EmptyChainLang(), "(list)").Result;
            Assert.IsInstanceOf(typeof(EmptyChain), result);
        }

        public class Empty1 { }

        public class EmptyChain { }

        [Language]
        public class EmptyChainLang
        {
            public EmptyChain Result { get; [Produce] set; }

            [Produce]
            public Empty1 Empty1() { return new Empty1(); }

            [Produce("(", "list", null, ")")]
            public EmptyChain List(Empty1 e1) { return new EmptyChain(); }
        }

    }
}
