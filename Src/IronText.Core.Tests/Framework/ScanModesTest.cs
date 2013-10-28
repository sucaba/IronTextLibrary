using System.Text;
using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class ScanModesTest
    {
        [Test]
        public void Test()
        {
            var lang = Language.Get(typeof(ScanModesLanguage));
            var context = new ScanModesLanguage();
            string commentText = "/* /* foo */ */ * / * bar";
            string text = "/*" + commentText + "*/";
            Assert.AreEqual(
                commentText,
                Language.Parse(context, text).Result);
        }

        [Language]
        // [GrammarDocument("ScanModes.gram")]
        // [ScannerDocument("ScanModes.scan")]
        [ScannerGraph("ScanModes_Scanner.gv")]
        public class ScanModesLanguage
        {
            [ParseResult] 
            public string Result { get; set; }

            [LanguageService]
            public IScanning Scanning { get; set; }

            [Literal("/*")]
            public void BeginComment(out CommentMode mode) 
            {
                mode = new CommentMode(this, Scanning);
            }
        }

        [Vocabulary]
        public class CommentMode
        {
            private StringBuilder comment;
            private int nestLevel = 1;
            private readonly ScanModesLanguage exit;
            private IScanning scanning;

            public CommentMode(ScanModesLanguage exit, IScanning scanning)
            {
                this.scanning = scanning;
                this.exit = exit;
                this.comment = new StringBuilder();
            }

            [Literal("/*")]
            public void BeginComment() 
            {
                ++nestLevel;
                comment.Append("/*");
            }

            [Scan("(~[*/] | '*' ~'/' | '/' ~'*') +")]
            public void Text(string text)
            {
                comment.Append(text);
            }

            [Literal("*/")]
            public string EndComment(out ScanModesLanguage mode)
            {
                if (--nestLevel == 0)
                {
                    mode = exit;
                    return comment.ToString();
                }
                else
                {
                    comment.Append("*/");
                    mode = null;
                    scanning.Skip();
                    return null;
                }
            }
        }
    }
}
