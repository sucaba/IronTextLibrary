using IronText.Runtime;
using IronText.Testing;
using NUnit.Framework;
using System;
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
            Assert.That(input, Dsl.ParsableBy<LeftRecursionWithBottomUpToken>());
        }

        [Test]
        [TestCase("aabaa")]
        public void NegativeTest(string input)
        {
            Assert.That(input, Dsl.Not.ParsableBy<LeftRecursionWithBottomUpToken>());
        }

        [Test]
        [TestCase("3")]
        [TestCase("3^3")]
        [TestCase("3^3^3")]
        public void AutoBottomUp(string input)
        {
            Assert.That(input, Dsl.ParsableBy<NondeterministicCalcWithAutoBottomUp>());
        }
    }
}
