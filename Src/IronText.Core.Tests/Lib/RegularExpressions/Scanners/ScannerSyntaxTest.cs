using IronText.Framework;
using IronText.Lib.ScannerExpressions;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Lib.ScannerExpressions
{
    /// <summary>
    /// A test class for ...
    /// </summary>
    [TestFixture]
    public class ScannerSyntaxTest
    {
        [Test]
        public void FirstTest()
        {
            Parse(@"Zs");
            Parse(@"Co");
            Parse(@"Lm");
            Parse(@"u0020");
            Parse(@"U0010ffff");
            Parse(@".");
            Parse(@".{12}");
            Parse(@".{12,}");
            Parse(@".{12,300}");
            Parse(@"';' ~[\r\n]*");
            Parse(@"'\r\n'");
            Parse(@"[ \t]+");
            Parse("('0'..'9')+  ('.' ('0'..'9')+)? | '.' ('0'..'9')+");
            Parse(@"
                    ('a'..'z' | 'A'..'Z' | [:.!@#$%^&|?*/+*=\\_-])
                    ('a'..'z' | 'A'..'Z' | '0'..'9' | [:.!@#$%^&|?*/+*=\\_-])?
                    ");
            Parse(@"
                    [""] 
                    ( ~[""\\]* ('\\' . ~[""\\]*)* )
                    [""]");
            Parse(@"'#\\' ('return' | 'linefeed' | '.')");
            Parse(@"~('a'..'z' | 'A' | [0123])");

            // Assert.Throws<SyntaxException>(() => Parse("'"), "Expected unclosed string");
            // Assert.Throws<SyntaxException>(() => Parse("["), "Expected unclosed character set");
            // Assert.Throws<SyntaxException>(() => Parse("]"), "Expected invalid character set close");
            Assert.Throws<SyntaxException>(() => Parse(".."), "Expected misplaced range dots");
            Assert.Throws<SyntaxException>(() => Parse("~"), "Expected misplaced complement operator");
            Assert.Throws<SyntaxException>(() => Parse("*"), "Expected misplaced star operator");
            Assert.Throws<SyntaxException>(() => Parse("+"), "Expected misplaced plus operator");
            Assert.Throws<SyntaxException>(() => Parse("?"), "Expected misplaced optional operator");
            Assert.Throws<SyntaxException>(() => Parse("Cx"), "Expected unknown identifier error");
        }

        private void Parse(string input)
        {
            var context = new ScannerSyntax();
            Language.Parse(context, input);
            Assert.IsNotNull(context.Result.Node);
        }
    }
}
