using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Samples
{
    [TestFixture]
    public class KeywordScannerTest
    {
        [Test]
        public void Test()
        {
            Language.Parse(new KeywordLang(), "print");
        }

        [Language]
        public class KeywordLang
        {
            [Produce("print")]
            public void All() { }
        }
    }
}
