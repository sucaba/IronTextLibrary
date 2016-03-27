using IronText.Framework;
using IronText.Logging;
using NUnit.Framework;

namespace IronText.Tests.IO
{
    /// <summary>
    ///This is a test class for SourceLocationTest and is intended
    ///to contain all SourceLocationTest Unit Tests
    ///</summary>
    [TestFixture]
    public class HLocTest
    {
        /// <summary>
        ///A test for SourceLocation Constructor
        ///</summary>
        [Test]
        public void SourceLocationCreateTest()
        {
            string filePath = @"c:\myFile.w";
            int character = 20;
            var target = new HLoc(filePath, character, character);
            Assert.AreEqual(filePath, target.FilePath);
            Assert.AreEqual(1, target.FirstLine);
            Assert.AreEqual(1, target.LastLine);
            Assert.AreEqual(character, target.FirstColumn);
            Assert.AreEqual(character, target.LastColumn);
        }

        [Test]
        public void GetHashTest()
        {
            string fileName = "myFile.wasp";
            int characterNumber = 893892;
            var target = new HLoc(fileName, characterNumber, characterNumber);
            var copy = new HLoc(fileName, characterNumber, characterNumber);

            Assert.AreEqual(target.GetHashCode(), copy.GetHashCode());
        }
    }
}
