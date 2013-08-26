using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class IntIntervalTest
    {
        [Datapoints]
        public int[] Values = new int[] { int.MinValue, 1, 2, 3, 4, 5, int.MinValue };

        [Datapoints]
        public IntInterval[] Intervals = new IntInterval[] 
        { 
            new IntInterval(1, 4),
            new IntInterval(2, 3),
            new IntInterval(3, 3),
            new IntInterval(int.MinValue, int.MaxValue),
            new IntInterval(int.MinValue, int.MinValue),
            new IntInterval(int.MaxValue, int.MaxValue),
            new IntInterval(3, 2),
        };

        [Theory]
        public void TestIntervalPosition(IntInterval interval, int value)
        {
            int first = interval.First;
            int last = interval.Last;

            checked
            {
                IntIntervalPosition position = interval.PositionOf(value);
                switch (position)
                {
                    case IntIntervalPosition.Less:
                        Assert.Less(value, first);
                        break;
                    case IntIntervalPosition.First:
                        Assert.AreEqual(value, first);
                        break;
                    case IntIntervalPosition.StrictlyInside:
                        Assert.IsTrue(last > value && value > first);
                        break;
                    case IntIntervalPosition.Last:
                        Assert.AreEqual(value, last);
                        break;
                    case IntIntervalPosition.Greater:
                        Assert.Greater(value, last);
                        break;
                    case IntIntervalPosition.Undefined:
                        Assert.IsTrue(interval.IsEmpty);
                        break;
                    default:
                        Assert.Fail("Invalid interval position result");
                        break;
                }

                if (position != IntIntervalPosition.Undefined)
                {
                    Assert.AreEqual(
                        last >= value && value >= first,
                        (position & IntIntervalPosition.InsideMask) != 0);

                    Assert.AreEqual(
                        last < value || value < first,
                        (position & IntIntervalPosition.OutsideMask) != 0);

                    Assert.AreEqual(
                        last == value || value == first,
                        (position & IntIntervalPosition.EdgeMask) != 0);
                }
            }
        }

        [Theory]
        public void TestIntervalRelation(int first2, int last2)
        {
            const int first1 = 1;
            const int last1 = 4;
            var interval1 = new IntInterval(first1, last1);
            var interval2 = new IntInterval(first2, last2);

            IntIntervalRelation relation = interval1.RelationTo(interval2);
            switch (relation)
            {
                case IntIntervalRelation.Less:
                    Assert.IsTrue(last1 < first2);
                    break;
                case IntIntervalRelation.Greater:
                    Assert.IsTrue(last2 < first1);
                    break;
                case IntIntervalRelation.Equal:
                    Assert.IsTrue(first1 == first2 && last1 == last2);
                    break;
                case IntIntervalRelation.Contained:
                    Assert.IsTrue(first2 <= first1 && last1 <= last2);
                    break;
                case IntIntervalRelation.Contains:
                    Assert.IsTrue(first1 <= first2 && last2 <= last1);
                    break;
                case IntIntervalRelation.OverlapFirst:
                    Assert.IsTrue(first1 <= first2 && first2 <= last1 && last2 > last1);
                    break;
                case IntIntervalRelation.OverlapLast:
                    Assert.IsTrue(first1 <= last2 && last2 <= last1 && last1 > last2);
                    break;
                default:
                    Assert.Fail("Unsupported relation");
                    break;
            }
        }
    }
}
