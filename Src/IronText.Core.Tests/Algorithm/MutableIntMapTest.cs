using System.Linq;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class MutableIntMapTest
    {
        const double D = 100;
        const double X = 10.5;
        const double Y = 3.14;
        const double Z = 2.22;

        [Test]
        public void Test()
        {
            var target = new MutableIntMap<double>();
            target.DefaultValue = D;
            Assert.AreEqual(D, target.DefaultValue);
            Assert.AreEqual(D, target.Get(0));

            target.Set(new IntArrow<double>(0, 5, X));
            Assert.AreEqual(X, target.Get(0));
            Assert.AreEqual(X, target.Get(5));

            target.Set(new IntArrow<double>(4, 5, Y));
            Assert.AreEqual(Y, target.Get(4));
            Assert.AreEqual(Y, target.Get(5));
            Assert.AreEqual(X, target.Get(0));
            Assert.AreEqual(X, target.Get(2));
            Assert.AreEqual(D, target.Get(-1));
            Assert.AreEqual(D, target.Get(1000));

            target.Clear(new IntInterval(3, 4));
            Assert.AreEqual(X, target.Get(0));
            Assert.AreEqual(X, target.Get(1));
            Assert.AreEqual(X, target.Get(2));
            Assert.AreEqual(D, target.Get(3));
            Assert.AreEqual(D, target.Get(4));
            Assert.AreEqual(Y, target.Get(5));
            Assert.AreEqual(D, target.Get(6));

            // No change:
            target.Clear(new IntInterval(int.MinValue, -1));
            Assert.AreEqual(X, target.Get(0));
            Assert.AreEqual(X, target.Get(1));
            Assert.AreEqual(X, target.Get(2));
            Assert.AreEqual(D, target.Get(3));
            Assert.AreEqual(D, target.Get(4));
            Assert.AreEqual(Y, target.Get(5));
            Assert.AreEqual(D, target.Get(6));

            // No change:
            target.Clear(new IntInterval(6, int.MaxValue));
            Assert.AreEqual(X, target.Get(0));
            Assert.AreEqual(X, target.Get(1));
            Assert.AreEqual(X, target.Get(2));
            Assert.AreEqual(D, target.Get(3));
            Assert.AreEqual(D, target.Get(4));
            Assert.AreEqual(Y, target.Get(5));
            Assert.AreEqual(D, target.Get(6));

            Assert.AreEqual(
                new [] 
                {
                    new IntArrow<double>(0, 2, X),
                    new IntArrow<double>(5, 5, Y),
                },
                target.Enumerate());

            target.Clear();
            for (int i = 0; i != 10; ++i)
            {
                Assert.AreEqual(D, target.Get(i), "context #" + i);
            }
        }

        [Test]
        public void WhenBoundsCoverMultipleArrowsEnumerateCoverage()
        {
            var target = new MutableIntMap<double>();
            target.DefaultValue = D;
            target.Set(new IntArrow<double>(0, 5, X));
            target.Set(new IntArrow<double>(9, 9, Y));
            target.Set(new IntArrow<double>(11, 14, Z));
            target.Set(new IntArrow<double>(20, 25, Y));

            var bounds = new IntInterval(3, 22);

            var expected = new[]
            {
                new IntArrow<double>(3, 5, X), 
                new IntArrow<double>(6, 8, D), 
                new IntArrow<double>(9, 9, Y),
                new IntArrow<double>(10, 10, D), 
                new IntArrow<double>(11, 14, Z),
                new IntArrow<double>(15, 19, D), 
                new IntArrow<double>(20, 22, Y),
            };

            var got = target.EnumerateCoverage(bounds).ToArray();

            Assert.AreEqual(expected, got);
        }

        [Test]
        public void WhenBoundsAreInsideSingleArrowEnumerateCoverage()
        {
            var target = new MutableIntMap<double>();
            target.DefaultValue = D;
            target.Set(new IntArrow<double>(0, 5, X));
            target.Set(new IntArrow<double>(9, 9, Y));
            target.Set(new IntArrow<double>(11, 14, Z));
            target.Set(new IntArrow<double>(20, 25, Y));

            var bounds = new IntInterval(21, 24);

            var expected = new[]
            {
                new IntArrow<double>(21, 24, Y),
            };

            var got = target.EnumerateCoverage(bounds).ToArray();

            Assert.AreEqual(expected, got);
        }
    }
}
