using IronText.Framework;
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
            [Parse("print")]
            public void All() { }
        }
    }
}
