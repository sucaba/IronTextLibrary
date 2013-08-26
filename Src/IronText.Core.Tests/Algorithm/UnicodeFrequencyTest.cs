using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class UnicodeFrequencyTest
    {
        [Test]
        [Explicit]
        public void TestDefault()
        {
            var defaultFrequency = UnicodeFrequency.Default;
            foreach (var entry in defaultFrequency.Enumerate().OrderByDescending(a => a.Value))
            {
                Debug.WriteLine("{0} : {1}", entry.Key, entry.Value);
            }

            Debug.WriteLine(
                "Ascii average fequency: {0}",
                defaultFrequency.Average(UnicodeIntSetType.AsciiInterval));
        }
    }
}
