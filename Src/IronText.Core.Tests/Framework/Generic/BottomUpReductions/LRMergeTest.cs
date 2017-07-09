using IronText.Testing;
using IronText.Tests.Extensions;
using NUnit.Framework;
using static IronText.Tests.Framework.Generic.GrammarsUnderTest;

namespace IronText.Tests.Framework.Generic.BottomUpReductions
{
    [TestFixture]
    public class LRMergeTest
    {
        [Test]
        [Explicit]
        public void Test()
        {
            Assert.That(
                "a" + "+a".Times(11),
                Dsl.ParsableBy<HighlyAmbiguousLanguage>());
        }
    }
}
