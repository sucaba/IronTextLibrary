using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.IO
{
    /// <summary>
    ///This is a test class for SourceLocationTest and is intended
    ///to contain all SourceLocationTest Unit Tests
    ///</summary>
    [TestFixture]
    public class LocTest
    {
        /// <summary>
        ///A test for SourceLocation Constructor
        ///</summary>
        [Test]
        public void SourceLocationCreateTest()
        {
            string filePath = @"c:\myFile.w";
            int character = 20;
            Loc target = new Loc(filePath, character);
            Assert.AreEqual(filePath, target.FilePath);
            Assert.AreEqual(character, target.Position);
        }

        [Test]
        public void EqualsTest()
        {
            string fileName = "myFile.w";
            int characterNumber = 893892;
            var target = new Loc(fileName, characterNumber);
            Assert.AreEqual(target, target);
            Assert.AreEqual(target, new Loc(fileName, characterNumber));
            Assert.AreNotEqual(target, new Loc(fileName + 1, characterNumber));
            Assert.AreNotEqual(target, new Loc(fileName, characterNumber + 1));
        }

        [Test]
        public void GetHashTest()
        {
            string fileName = "myFile.wasp";
            int characterNumber = 893892;
            var target = new Loc(fileName, characterNumber);
            Assert.AreEqual(target.GetHashCode(), new Loc(fileName, characterNumber).GetHashCode());
        }
    }
}
