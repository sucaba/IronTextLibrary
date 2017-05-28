using IronText.Testing;
using NUnit.Framework;
using static IronText.Tests.Framework.Generic.GrammarsUnderTest;

namespace IronText.Tests.Framework.Generic
{
    [TestFixture]
    public class BottomUpTest
    {
        [Test]
        [TestCase("")]
        [TestCase("a")]
        [TestCase("aa")]
        [TestCase("aaa")]
        [TestCase("aaaa")]
        [TestCase("aaaaa")]
        [TestCase("aaaaaa")]
        [TestCase("aaaaaaaaaa")]
        public void PositiveTest(string input)
        {
            Assert.That(input, StructuredText.Is.ParsableBy<WithBottomUpToken>());
        }

        [Test]
        [TestCase("aabaa")]
        public void NegativeTest(string input)
        {
            Assert.That(input, StructuredText.Is.Not.ParsableBy<WithBottomUpToken>());
        }
    }
}
