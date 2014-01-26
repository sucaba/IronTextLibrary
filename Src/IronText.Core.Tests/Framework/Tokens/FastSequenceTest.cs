using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Framework;
using IronText.Lib;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework.Tokens
{
    [TestFixture]
    public class FastSequenceTest
    {
        [Datapoints]
        public static TestInfo[] TestInfos = new[]{
            new TestInfo { RepeatCount = 100000, ItemCount = 1 },
            new TestInfo { RepeatCount = 100000, ItemCount = 2 },
            new TestInfo { RepeatCount = 100000, ItemCount = 3 },
            new TestInfo { RepeatCount = 100000, ItemCount = 4 },
            new TestInfo { RepeatCount = 100000, ItemCount = 5 },
            new TestInfo { RepeatCount = 100000, ItemCount = 6 },
            new TestInfo { RepeatCount = 100000, ItemCount = 10 },
            new TestInfo { RepeatCount = 100000, ItemCount = 20 },
            };

        [Theory]
        [Explicit]
        public void Test(TestInfo testInfo)
        {
            int R = testInfo.RepeatCount;
            int N = testInfo.ItemCount;

            var context = new FastSeqLang();
            var lang = Language.Get(typeof(FastSeqLang));

            var ITEM = lang.Token<string>("foo");

            Debug.WriteLine(testInfo.Name + " performance results:");

            var timer = new Stopwatch();
            foreach (var tag in new[] { "list", "array" })
            {
                var prefix = lang.Literal(tag);

                timer.Reset();

                int i = R;
                while (i-- != 0)
                {
                    timer.Start();
                    var output = Language.Parse(context, PrefixedRepeat(prefix, ITEM, N)).Result;
                    timer.Stop();
                    Assert.AreEqual(N, output.Items.Count());
                }

                Debug.WriteLine("  " + tag + " : " + timer.Elapsed);
            }
        }

        private static IEnumerable<T> PrefixedRepeat<T>(T prefix, T item, int n)
        {
            yield return prefix;
            while (n-- != 0)
            {
                yield return item;
            }
        }

        public struct Coll<T>
        {
            public IList<T> Items;
        }

        [Language]
        [StaticContext(typeof(Builtins))]
        // [ScannerDocument("FastSeqLang.scan")]
        // [DescribeParserStateMachine("FastSeqLang.info")]
        public class FastSeqLang
        {
            [ParseResult]
            public Coll<string> Result { get; set; }

            [Parse("list")]
            public static Coll<string> List2Array(List<string> items)
            { 
                return new Coll<string> { Items = items };  
            }

            [Parse("array")]
            public static Coll<string> Array2Array(string[] items)
            { 
                return new Coll<string> { Items = items };  
            }

            [Literal("foo")]
            public static string FakeScanRule() { throw new NotImplementedException(); }
        }

        public class TestInfo
        {
            public int RepeatCount;
            public int ItemCount;

            public string Name
            {
                get { return string.Format("[R={0}, N={1}]", RepeatCount, ItemCount); }
            }
        }

    }
}
