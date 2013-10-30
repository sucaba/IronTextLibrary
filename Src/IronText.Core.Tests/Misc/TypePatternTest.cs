using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Misc;
using NUnit.Framework;

namespace IronText.Tests.Misc
{
    [TestFixture]
    public class TypePatternTest
    {
        [Test]
        public void PatternMatchesType()
        {
            var matcher = typeof(IRules).GetMethod("Rule1");
            var target = new TypePattern(matcher);

            {
                var type = typeof(Dictionary<Dictionary<int, string>, Dictionary<int, byte>>);
                Type[] types = target.Match(type);
                Assert.AreEqual(new[] { typeof(int), typeof(string), typeof(byte) }, types);
                Assert.IsNotNull(target.MakeProducer(type));
            }

            {
                // 2 types for the same type parameter
                var type = typeof(Dictionary<Dictionary<int, string>, Dictionary<char, byte>>);
                Type[] types = target.Match(type);
                Assert.IsNull(types);
                Assert.IsNull(target.MakeProducer(type));
            }
        }

        interface IRules
        {
            Dictionary<Dictionary<T1, T2>, Dictionary<T1, T3>> Rule1<T1,T2,T3>();
        }

        public class Opt<T> { }
    }
}
