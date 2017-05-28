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
            Assert.That(input, StructuredText.Is.ParsableBy<LeftRecursionWithBottomUpToken>());
        }

        [Test]
        [TestCase("aabaa")]
        public void NegativeTest(string input)
        {
            Assert.That(input, StructuredText.Is.Not.ParsableBy<LeftRecursionWithBottomUpToken>());
        }

        [Test]
        [TestCase("3")]
        [TestCase("3^3")]
        [TestCase("3^3^3")]
        public void AutoBottomUp(string input)
        {
            Assert.That(input, StructuredText.Is.ParsableBy<NondeterministicCalcWithAutoBottomUp>());
        }
    }
}
