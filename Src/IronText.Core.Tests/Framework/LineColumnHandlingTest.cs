using System.Collections.Generic;
using System.Linq;
using IronText.Framework;
using IronText.Lib;
using IronText.Logging;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class LineColumnHandlingTest
    {
        [Test]
        public void Test()
        {
            var context = new LCLang();
            using (var interp = new Interpreter<LCLang>(context))
            {
                var lang = Language.Get(typeof(LCLang));
                string text = "at-1\r\natNL-2\nat-3\nat-4\nbegin-5\r\n\n\r\nend-8\r\nat-9";
                var hlocs = interp.Scan(text).Select(msg => msg.HLocation).ToArray();
                Assert.AreEqual(context.Result[0], hlocs[0]);
                Assert.AreEqual(context.Result[1], hlocs[1]);
                Assert.AreEqual(context.Result[2], hlocs[2]);
                Assert.AreEqual(context.Result[3], hlocs[3]);
                Assert.AreEqual(context.Result[4], hlocs[4]);
            }
        }

        [Language]
//        [ScannerGraph("LCLang_Scanner.gv")]
        [StaticContext(typeof(Builtins))]
        public class LCLang
        {
            public readonly List<HLoc> Result = new List<HLoc>();

            [LanguageService]
            public IScanning Scanning { get; set; }

            [Produce]
            public void All(List<HLoc> lines) { }

            [Match(@"
                'begin-' digit 
                ('\r'? '\n')*
                'end-' digit")]
            public HLoc MultiLineTerm(string text)
            {
                int length = text.Length;

                int prefix = "begin-".Length;
                int suffix = "end-".Length;
                int innerLineCount = (length - prefix - suffix - 2) / 2;

                int expectedFirstLine = (text[prefix] - '0');
                int expectedFirstColumn = 1;
                int expectedLastLine = (text[length - 1] - '0');
                int expectedLastColumn = suffix + 1;

                var result = new HLoc(
                        expectedFirstLine,
                        expectedFirstColumn,
                        expectedLastLine,
                        expectedLastColumn);
                Result.Add(result);

                return result;
            }

            [Match("'at-' digit")]
            public HLoc SingleLineTerm(string text)
            {
                int length = text.Length;

                int expectedLine = (text[length - 1] - '0');
                int expectedLastColumn = length;
                var result = new HLoc(
                        expectedLine,
                        1,
                        expectedLine,
                        expectedLastColumn);
                Result.Add(result);

                return result;
            }

            [Match("'atNL-' digit '\n'")]
            public HLoc SingleLineNLTerm(string text)
            {
                int length = text.Length;

                int expectedLine = (text[length - 2] - '0');
                int expectedLastColumn = length;
                var result = new HLoc(
                        expectedLine,
                        1,
                        expectedLine,
                        expectedLastColumn);
                Result.Add(result);

                return result;
            }

            [Match("'\r'? '\n'")]
            public void Newline()
            {
            }
        }
    }
}
