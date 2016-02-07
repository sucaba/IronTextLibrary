using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class IntervalIntSetTest
    {
        static IntSetType IntSet = SparseIntSetType.Instance;

        [Datapoints]
        public int[] Values = new int[] { 0, 1000, 1, IntSet.MinValue, IntSet.MaxValue };

        [Datapoints]
        public int[][] EnumeratedValues = new int[][] { 
            new int[0],
            new int[] { 0 },
            new int[] { 1 },
            new int[] { IntSet.MaxValue },
            new int[] { IntSet.MinValue },
            new int[] { IntSet.MaxValue, IntSet.MinValue, IntSet.MaxValue },
            new int[] { 0, 1, IntSet.MinValue, IntSet.MaxValue },
        };

        [Datapoints]
        public IntInterval[][] RangesSequences = new IntInterval[][] {
            new IntInterval[0],
            new IntInterval[] { new IntInterval(IntSet.MinValue, IntSet.MaxValue) },
            new IntInterval[] { new IntInterval(0, IntSet.MaxValue) },
            new IntInterval[] { new IntInterval(IntSet.MinValue, 0) },
            new IntInterval[] { new IntInterval(0, 0) },
            new IntInterval[] { new IntInterval(IntSet.MinValue, IntSet.MinValue) },
            new IntInterval[] { new IntInterval(IntSet.MaxValue, IntSet.MaxValue) },
            new IntInterval[] { 
                 new IntInterval(IntSet.MinValue, IntSet.MaxValue),
                 new IntInterval(0, IntSet.MaxValue),
                 new IntInterval(IntSet.MinValue, 0),
                 new IntInterval(0, 0),
                 new IntInterval(IntSet.MinValue, IntSet.MinValue),
                 new IntInterval(IntSet.MaxValue, IntSet.MaxValue),
            }
        };

        [Datapoints]
        public IntSet[] Sets = new IntSet[] {
            IntSet.Empty,
            IntSet.Of(IntSet.MaxValue),
            IntSet.Of(0),
            IntSet.Of(5),
            IntSet.Of(5),
            IntSet.Of(10),
            IntSet.Of(21),
            IntSet.Of(100),
            IntSet.Of(10),
            IntSet.Of((int)short.MaxValue),
            IntSet.Of((int)char.MaxValue),
            IntSet.Range(0, 100),
            IntSet.Range(10, 10),
            IntSet.Range(10, 9),
//            IntSet.Range(0, IntSet.MaxValue),
//            IntSet.Range(IntSet.MinValue, IntSet.MaxValue),
            IntSet.Of(new int[] { 0, 1, IntSet.MinValue, IntSet.MaxValue }),
            IntSet.Ranges(
                new [] { 
                     new IntInterval(10, 10),
                     new IntInterval(10, 10),
                     new IntInterval(10, 10),
                }),
            IntSet.Ranges(
                new [] { 
                     new IntInterval(00, 10),
                     new IntInterval(11, 20),
                     new IntInterval(30, 40),
                     new IntInterval(100, 100),
                }),
            IntSet.Ranges(
                new [] { 
                     new IntInterval(1, 11),
                     new IntInterval(19, 26),
                     new IntInterval(28, 29),
                     new IntInterval(41, 99),
                }),
            IntSet.Ranges(
                new IntInterval[] { 
                 new IntInterval(0, 0),
                 new IntInterval(0, 100),
                 new IntInterval(IntSet.MinValue, IntSet.MinValue),
                 new IntInterval(IntSet.MaxValue, IntSet.MaxValue),
            }),
        };

        [Theory]
        public void singleton_set_contains_only_its_value(int value)
        {
            var target = IntSet.Of(value);
            Assert.IsTrue(target.Contains(value));
            foreach (int sample in AnyBut(value))
            {
                Assert.IsFalse(target.Contains(sample));
            }
        }

        [Theory]
        public void single_range_set_contains_only_elements_in_range(int start, int finish)
        {
            Assume.That(finish >= start);

            int middle = (int)(((long)start + finish) / 2);

            var target = IntSet.Range(start, finish);
            Assert.IsTrue(target.Contains(start));
            Assert.IsTrue(target.Contains(middle));
            Assert.IsTrue(target.Contains(finish));

            foreach (int value in ExceptRange(start, finish))
            {
                Assert.IsFalse(target.Contains(value));
            }
        }

        [Theory]
        public void enumerated_values_set_contains_only_these_values(int[] values)
        {
            var target = IntSet.Of(values);
            foreach (int value in values)
            {
                Assert.IsTrue(target.Contains(value));
            }

            foreach (int value in ExceptValues(values))
            {
                Assert.IsFalse(target.Contains(value));
            }
        }

        [Theory]
        public void adding_existing_values_to_set_does_not_change_set(IntSet x, int value)
        {
            x = x.Union(IntSet.Of(value));
            int countBeforeValueDuplication = x.Count;
            Assert.IsTrue(x.Contains(value));
            x.Union(IntSet.Of(value));
            Assert.IsTrue(x.Contains(value));

            Assert.AreEqual(countBeforeValueDuplication, x.Count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void creating_set_from_null_intervals_array_causes_exception()
        {
            var ignored = IntSet.Ranges((IntInterval[])null);
        }

        [Theory]
        public void set_from_intervals_array_contains_only_element_in_these_ranges(IntInterval[] intervals)
        {
            var target = IntSet.Ranges(intervals);
            foreach (var interval in intervals)
            {
                Assert.IsTrue(target.Contains(interval.First));
                Assert.IsTrue(target.Contains(interval.Last));
                int middle = (int)(((long)interval.First + interval.Last) / 2);
                Assert.IsTrue(target.Contains(middle));
            }

            foreach (int value in ExceptValues(intervals))
            {
                Assert.IsFalse(target.Contains(value));
            }
        }

        [Theory]
        public void set_union_returns_correct_result(IntSet x, IntSet y)
        {
            var union = x.Union(y);
            var hsUnion = new HashSet<int>(x);
            hsUnion.UnionWith(new HashSet<int>(y));
            CollectionAssert.AreEquivalent(hsUnion, union);

            set_is_optimized(union);
        }

        [Theory]
        public void set_intersection_returns_correct_result(IntSet x, IntSet y)
        {
            var intersection = x.Intersect(y);
            var hsIntersection = new HashSet<int>(x);
            hsIntersection.IntersectWith(new HashSet<int>(y));
            CollectionAssert.AreEquivalent(hsIntersection, hsIntersection);

            set_is_optimized(intersection);
        }

        [Theory]
        public void set_is_optimized(IntSet set)
        {
            // TODO: ??
        }

        [Theory]
        public void set_reports_correct_count(IntSet set)
        {
            Assert.AreEqual(set.Count(), set.Count);
        }

        [Test]
        public void identical_sets_are_equal()
        {
            var sets = new IntSet[]
            {
                IntSet.Of(10),
                IntSet.Of(10),
                IntSet.Range(10, 10),
                IntSet.Range(10, 10),
                IntSet.Ranges(
                    new []
                    {
                        new IntInterval(10, 10),
                        new IntInterval(10, 10),
                        new IntInterval(10, 10),
                    }),
            };

            for (int i = 0; i != sets.Length; ++i)
            {
                var x = sets[i];
                for (int j = 0; j != sets.Length; ++j)
                {
                    var y = sets[j];
                    Assert.IsTrue(x.Equals(y));
                    Assert.IsTrue(y.Equals(x));
                    Assert.IsTrue(x.SetEquals(y));
                    Assert.IsTrue(y.SetEquals(x));
                }
            }
        }

        [Theory]
        public void equility_and_hash_works_according_to_the_content(IntSet x, IntSet y)
        {
            bool equals = x.Equals(y);
            bool setEquals = x.SetEquals(y);
            bool hsEquals = new HashSet<int>(x).SetEquals(new HashSet<int>(y));

            Assert.AreEqual(hsEquals, equals);
            Assert.AreEqual(hsEquals, setEquals);
            if (equals)
            {
                Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
            }
        }

        [Theory]
        public void complement_returns_correct_result(IntSet set)
        {
            var got = set.Complement();
            foreach (var item in set)
            {
                Assert.IsTrue(!got.Contains(item));
            }

            int countToCheck = 1000;
            foreach (var item in got)
            {
                Assert.IsFalse(set.Contains(item));
                if (--countToCheck == 0)
                {
                    break;
                }
            }

            set_is_optimized(got);
        }

        [Theory]
        public void set_has_correct_content_after_adding_value(IntSet set, int value)
        {
            MutableIntSet editedSet = set.EditCopy();
            Assert.AreEqual(set.Contains(value), editedSet.Contains(value));
            editedSet.Add(value);
            Assert.IsTrue(editedSet.Contains(value));

            Assert.IsTrue(editedSet.Contains(value));
            Assert.IsTrue(set.Union(IntSet.Of(value)).SetEquals(editedSet));
            IntSet result = editedSet.CompleteAndDestroy();
            Assert.IsTrue(result.Contains(value));
            Assert.IsTrue(set.IsSubsetOf(result));
            Assert.IsTrue(set.Union(IntSet.Of(value)).SetEquals(result));
        }

        [Theory]
        public void set_has_correct_content_after_removing_value(IntSet set, int value)
        {
            MutableIntSet editedSet = set.EditCopy();
            int initialCount = editedSet.Count;
            editedSet.Remove(value);
            int finalCount = editedSet.Count;
            Assert.IsFalse(editedSet.Contains(value));
            Assert.IsTrue(finalCount == initialCount || finalCount + 1 == initialCount);
        }

        [Theory]
        public void set_has_correct_content_after_popany_value(IntSet set)
        {
            Assume.That(!set.IsEmpty);

            MutableIntSet editedSet = set.EditCopy();
            int initialCount = editedSet.Count;
            int value = editedSet.PopAny();
            int finalCount = editedSet.Count;
            Assert.IsFalse(editedSet.Contains(value));
            Assert.AreEqual(initialCount, finalCount + 1);
        }

        [Theory]
        public void set_has_correct_content_after_adding_set(IntSet set, IntSet other)
        {
            MutableIntSet editedSet = set.EditCopy();
            editedSet.AddAll(other);
            Assert.IsTrue(editedSet.IsSupersetOf(other));
            Assert.IsTrue(set.Union(other).SetEquals(editedSet));
            IntSet result = editedSet.CompleteAndDestroy();
            Assert.IsTrue(result.IsSupersetOf(other));
            Assert.IsTrue(set.Union(other).SetEquals(result));
        }

        [Theory]
        public void adding_element_to_the_mutable_set(int value)
        {
            var target = IntSet.Mutable();
            target.Add(value);
            Assert.IsTrue(target.Contains(value));
            Assert.AreEqual(1, target.Count);
        }

        [Theory]
        public void immutable_instance_is_not_castable_to_mutable(IntSet instance)
        {
            Assert.IsNotInstanceOf<MutableIntSet>(instance);
        }

        private IEnumerable<int> ExceptValues(IntInterval[] intervals)
        {
            foreach (var value in Values)
            {
                if (!intervals.Any(r => value >= r.First && value <= r.Last))
                {
                    yield return value;
                }
            }
        }

        private IEnumerable<int> ExceptValues(int[] values)
        {
            if (values.Length == 0)
            {
                return Values;
            }

            if (values.Length == 1)
            {
                return AnyBut(values[0]);
            }

            var result = new List<int>();

            if (values.Min() != IntSet.MinValue)
            {
                result.Add(IntSet.MinValue);
            }

            if (values.Max() != IntSet.MaxValue)
            {
                result.Add(IntSet.MaxValue);
            }

            for (int i = 1; i != values.Length; ++i)
            {
                int x = values[i - 1];
                int y = values[i];
                if (x == y)
                {
                    continue;
                }

                int middle = (int)(((long)x + y) / 2);
                if (values.Contains(middle))
                {
                    continue;
                }

                result.Add(middle);
            }

            return result;
        }

        private IEnumerable<int> ExceptRange(int start, int finish)
        {
            if (start != IntSet.MinValue)
            {
                yield return start - 1;
            }

            if (finish != IntSet.MaxValue)
            {
                yield return finish + 1;
            }
        }

        private IEnumerable<int> AnyBut(int value) { return ExceptRange(value, value); }
    }
}
