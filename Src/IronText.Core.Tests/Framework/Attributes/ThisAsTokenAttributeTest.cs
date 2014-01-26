using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework.Attributes
{
    /// <summary>
    /// A test class for <see cref="DemandAttribute"/>
    /// </summary>
    [TestFixture]
    public class ThisAsTokenAttributeTest
    {
        [Test]
        public void InterfaceAsRootContext()
        {
            string input = "3 add 100 remove 10 add 1 end";
            double result = Language.Parse(new RootContext(), input).Result;
            Assert.AreEqual(94, result);
        }

        [Language]
        public class RootContext
        {
            public double Result { get; [Parse] set; }

            [Parse]
            public Accumulator Seed(int initial) { return new Accumulator(initial); }

            [Scan("digit+")]
            public int Number(string text) { return int.Parse(text); }

            [Scan("blank+")]
            public void Space() { }
        }

        [Demand]
        public class Accumulator
        {
            private  int value;

            public Accumulator() : this(0) { }

            public Accumulator(int initial)
            {
                value = initial;
            }

            [Parse("add")]
            public Accumulator Add(int x) { value += x; return this; }

            [Parse("remove")]
            public Accumulator Remove(int x) { value -= x; return this; }

            public interface Start { }

            [Parse("end")]
            public double End() { return value; }

            //[Pattern("end")]
            //public Start Close() { return default(Start); }
        }
    }
}
