using IronText.Framework;
using IronText.Reflection;
using IronText.Runtime;
using NUnit.Framework;
using System;
using static IronText.Tests.Framework.Generic.GrammarsUnderTest;

namespace IronText.Tests.Framework.Generic
{
    [TestFixture]
    public class GenericMergeTest
    {
        [Test]
        public void TrivialMerge()
        {
            var r = Language.Parse(new TrivialMergeLanguage(), "a").Result;
            Assert.That(r, Has.Count.EqualTo(1));
        }

        [Test]
        public void ResolvingAmbiguities()
        {
            using (var interp = new Interpreter<AmbiguousCalculator>())
            {
                interp.Parse("2+8/2");
                Assert.AreEqual(6, interp.Context.Result.Value);

                interp.Parse("8/4/2");
                Assert.AreEqual(1, interp.Context.Result.Value);

                interp.Parse("2+3"); // can be interpreted as a "2 * (+3)"
                Assert.AreEqual(5, interp.Context.Result.Value);

                // Check that implicit multiplication works
                interp.Parse("2 3"); 
                Assert.AreEqual(6, interp.Context.Result.Value);

                // A lot of ambiguities:
                interp.Parse("1+-+6/3"); 
                Assert.AreEqual(-1, interp.Context.Result.Value);
            }
        }
    }
}
