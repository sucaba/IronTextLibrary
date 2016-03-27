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
            using (var interp = new Interpreter<LCLang>())
            {
                string text = "at-1\r\natNL-2\nat-3\nat-4\nbegin-5\r\n\n\r\nend-8\r\nat-9";
                //             0123 4 5678901 23456 78901 23456789 0 1 2 3 456789 0 123456
                //             0           1           2           3              4
                // lines:
                //         00: xxxx
                //         01:         xxxxxx_x
                //         02:                 xxxx
                //         03:                       xxxx
                //         04:                             xxxxxxx
                //         05:                                        |   
                //         06:                                          |   
                //         07:                                              xxxxx   
                //         08:                                                       xxxx   
                
                var hlocs = interp.Scan(text).Select(msg => msg.Location).ToArray();
                Assert.AreEqual(new Loc(1,1,1,4), hlocs[0]);
                Assert.AreEqual(new Loc(2,1,2,7), hlocs[1]);
                Assert.AreEqual(new Loc(3,1,3,4), hlocs[2]);
                Assert.AreEqual(new Loc(4,1,4,4), hlocs[3]);
                Assert.AreEqual(new Loc(5,1,8,5), hlocs[4]);
                Assert.AreEqual(new Loc(9,1,9,4), hlocs[5]);
            }
        }

        [Language]
//        [ScannerGraph("LCLang_Scanner.gv")]
        [StaticContext(typeof(Builtins))]
        public interface LCLang
        {
            [Produce]
            void All(List<object> lines);

            [Match(@"
                'begin-' digit 
                ('\r'? '\n')*
                'end-' digit")]
            object MultiLineTerm(string text);

            [Match("'at-' digit")]
            object SingleLineTerm(string text);

            [Match("'atNL-' digit '\n'")]
            object SingleLineNLTerm(string text);

            [Match("'\r'? '\n'")]
            void Newline();
        }
    }
}
