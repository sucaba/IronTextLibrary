using IronText.Framework;
using IronText.Lib.Sre;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Lib.Sre
{
    [TestFixture]
    public class ParserTest
    {
        [Test]
        public void NativeExpamplesTest()
        {
            string[] examples = {
                @"(- alpha (""aeiouAEIOU""))		// Various forms of non-vowel letter",
                @"(- alpha (""aeiou"") (""AEIOU""))",
                @"(w/nocase (- alpha (""aeiou"")))",
                @"(- (/""azAZ"") (""aeiouAEIOU""))",
                @"(w/nocase (- (/""az"") (""aeiou"")))",
                @"(| upper (""aeiou"") digit) // Upper-case letter, lower-case vowel, or digit",
                @"(| (/""AZ09"") (""aeiou""))",
                @"(| ""John"" ""Paul"" ""George"" ""Ringo"")"
                };

            foreach (var example in examples)
            {
                Language.Parse(new SreSyntax(), example);
            }
        }

        [Test]
        public void EscapedCharactersTest()
        {
            string[] examples = {
                @"""s\""tr""", // escaped quote
                @"""s\\tr""", // escaped back slash
                };

            foreach (var example in examples)
            {
                Language.Parse(new SreSyntax(), example);
            }
        }
    }
}
