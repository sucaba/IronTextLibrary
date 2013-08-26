using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class UnicodeIntSetTypeTest
    {
        UnicodeIntSetType Unicode = UnicodeIntSetType.Instance;
        UnicodeIntSetType Ascii = UnicodeIntSetType.Instance;

        [Test]
        public void digit_sets_have_correct_content()
        {
            var decimals = Unicode.Range('0', '9');
            Assert.IsTrue(Unicode.DecimalDigitNumber.IsSupersetOf(decimals));
            Assert.IsTrue(Unicode.AsciiDigit.SetEquals(decimals));
        }

        [Test]
        public void alpha_sets_have_correct_content()
        {
            var lowerAsciiLetter = Unicode.Range('a', 'z');
            var upperAsciiLetter = Unicode.Range('A', 'Z');
            var asciiLetter = lowerAsciiLetter.Union(upperAsciiLetter);

            Assert.AreEqual(lowerAsciiLetter, Unicode.AsciiLower);
            Assert.AreEqual(upperAsciiLetter, Unicode.AsciiUpper);
            Assert.AreEqual(asciiLetter, Unicode.AsciiAlpha);

            Assert.IsTrue(Unicode.LowercaseLetter.IsSupersetOf(lowerAsciiLetter));
            Assert.IsFalse(Unicode.LowercaseLetter.Overlaps(upperAsciiLetter));
            Assert.IsTrue(Unicode.UppercaseLetter.IsSupersetOf(upperAsciiLetter));
            Assert.IsFalse(Unicode.UppercaseLetter.Overlaps(lowerAsciiLetter));

            Assert.IsTrue(Unicode.Letter.IsSupersetOf(upperAsciiLetter));
            Assert.IsTrue(Unicode.Letter.IsSupersetOf(lowerAsciiLetter));
        }
    }
}
