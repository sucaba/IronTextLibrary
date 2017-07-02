using IronText.Testing;
using NUnit.Framework;
using static IronText.Tests.Framework.Generic.GrammarsUnderTest;

namespace IronText.Tests.Framework.Generic
{
    [TestFixture]
    public class BottomUpConsumesTopDownResultTest
    {
        [Test]
        public void Test()
        {
            Assert.That("bae", Dsl.ParsableBy<BottomUpConsumesTopDownLanguage>());           
        }
    }
}
