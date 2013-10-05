using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IronText.Algorithm
{
    public class MutableIntMap<TAttr> : IMutableIntMap<TAttr>
        where TAttr : IEquatable<TAttr>
    {
        protected readonly List<IntArrow<TAttr>> arrows;
        private TAttr defaultAttribute;

        public MutableIntMap()
        {
            this.arrows = new List<IntArrow<TAttr>>();
        }

        public MutableIntMap(IEnumerable<IntArrow<TAttr>> arrows)
            : this(arrows, default(TAttr))
        {
        }

        public MutableIntMap(IEnumerable<IntArrow<TAttr>> arrows, TAttr defaultAttribute)
        {
            // Set in reverse order because firsts are more important i.e. overriding
            this.arrows = new List<IntArrow<TAttr>>();
            foreach (var arrow in arrows.Reverse())
            {
                Set(arrow);
            }

            this.defaultAttribute = defaultAttribute;
        }

        public TAttr DefaultValue
        {
            get { return defaultAttribute; }
            set { defaultAttribute = value; }
        }

        public IntInterval Bounds
        {
            get
            {
                return arrows.Aggregate(IntInterval.Empty, (acc, arrow) => acc.Union(arrow.Key));
            }
        }

        public TAttr Get(int value)
        {
            int count = arrows.Count;
            // TODO: Binary search over sorted arrows
            for (int i = 0; i != count; ++i)
            {
                if (arrows[i].Key.Contains(value))
                {
                    return arrows[i].Value;
                }
            }

            return defaultAttribute;
        }

        public IEnumerable<IntArrow<TAttr>> Enumerate(IntInterval bounds)
        {
            if (arrows.Count == 0 || bounds.IsEmpty)
            {
                yield break;
            }

            int i = 0;
            int count = arrows.Count;
            while (i != count && bounds.First > arrows[i].Key.Last)
            {
                ++i;
            }

            while (i != count && bounds.Contains(arrows[i].Key))
            {
                yield return arrows[i];
                ++i;
            }

            if (i != count)
            {
                var intersection = arrows[i].Key * bounds;
                if (!intersection.IsEmpty)
                {
                    yield return new IntArrow<TAttr>(intersection, arrows[i].Value);
                }
            }
        }

        public IEnumerable<IntArrow<TAttr>> Enumerate()
        {
            return arrows;
        }

        public void Set(IntervalIntSet set, TAttr attr)
        {
            foreach (var interval in set.EnumerateIntervals())
            {
                Set(new IntArrow<TAttr>(interval, attr));
            }
        }

        public void Set(IntArrow<TAttr> newArrow)
        {
#if false
            if (newArrow.Value.Equals(DefaultValue))
            {
                Clear(newArrow.Key);
                return;
            }
#endif

            IntInterval bounds = newArrow.Key;
            var relation = ParseBoundsRelation(bounds);

            if (relation.ContainsIndex >= 0)
            {
                int i = relation.ContainsIndex;

                var oldArrow = arrows[i];

                if (oldArrow.Value.Equals(newArrow.Value))
                {
                    return;
                }

                var before = oldArrow.Before(bounds);
                if (!before.IsEmpty)
                {
                    arrows.Insert(i, before);
                    ++i;
                }

                arrows[i] = newArrow;
                ++i;

                var after = oldArrow.After(bounds);
                if (!after.IsEmpty)
                {
                    arrows.Insert(i, after);
                }

                return;
            }

            if (relation.FirstOverlapIndex >= 0)
            {
                int i = relation.FirstOverlapIndex;
                arrows[i] = arrows[i].Before(bounds);
            }

            int shift = 0;

            Debug.Assert(relation.ReplaceIndex >= 0);
            arrows.Insert(relation.ReplaceIndex, newArrow);
            ++shift;

            foreach (int containedIndex in relation.ContainedIndexes)
            {
                arrows.RemoveAt(containedIndex + shift);
                --shift;
            }

            if (relation.LastOverlapIndex >= 0)
            {
                arrows[relation.LastOverlapIndex + shift] 
                    = arrows[relation.LastOverlapIndex + shift].After(bounds);
            }
        }

        public void Clear(IntInterval bounds)
        {
            var relation = ParseBoundsRelation(bounds);

            if (relation.ContainsIndex >= 0)
            {
                var before = arrows[relation.ContainsIndex].Before(bounds);
                var after = arrows[relation.ContainsIndex].After(bounds);
                if (!before.Key.IsEmpty)
                {
                    arrows[relation.ContainsIndex] = before;
                }

                if (!after.Key.IsEmpty)
                {
                    if (before.IsEmpty)
                    {
                        arrows[relation.ContainsIndex] = after;
                    }
                    else
                    {
                        arrows.Insert(relation.ContainsIndex + 1, after);
                    }
                }

                return;
            }

            if (relation.FirstOverlapIndex >= 0)
            {
                arrows[relation.FirstOverlapIndex] 
                    = arrows[relation.FirstOverlapIndex].Before(bounds);
            }

            if (relation.LastOverlapIndex >= 0)
            {
                arrows[relation.LastOverlapIndex] 
                    = arrows[relation.LastOverlapIndex].After(bounds);
            }

            int shift = 0;
            foreach (int containedIndex in relation.ContainedIndexes)
            {
                arrows.RemoveAt(containedIndex + shift);
                --shift;
            }
        }

        public void Clear()
        {
            arrows.Clear();
        }

        protected BoundsRelation ParseBoundsRelation(IntInterval bounds)
        {
            var result = new BoundsRelation();

            if (arrows.Count == 0 || bounds.IsEmpty)
            {
                result.ReplaceIndex = 0;
                return result;
            }

            int i = 0;
            int count = arrows.Count;

            IntIntervalRelation relation;

            // Skip 'Less*'
            while (LookAhead(i, bounds) == IntIntervalRelation.Less)
            {
                ++i;
            }

            // 'OverlapFirst?'
            if (LookAhead(i, bounds) == IntIntervalRelation.OverlapFirst)
            {
                result.FirstOverlapIndex = i++;
            }

            result.ReplaceIndex = i;

            while (true)
            {
                relation = LookAhead(i, bounds);
                switch (relation)
                {
                    case IntIntervalRelation.Undefined:
                    case IntIntervalRelation.Greater:
                        return result;
                    // 'OverlapLast?'
                    case IntIntervalRelation.OverlapLast:
                        result.LastOverlapIndex = i;
                        ++i;
                        return result;
                    case IntIntervalRelation.Equal:
                    case IntIntervalRelation.Contains:
                        result.ContainsIndex = i;
                        return result;
                    case IntIntervalRelation.Contained:
                        result.ContainedIndexes.Add(i);
                        ++i;
                        continue;
                    default:
                        Debug.Fail("Internal error");
                        break;
                }
            }
        }
             
        private IntIntervalRelation LookAhead(int i, IntInterval bounds)
        {
            if (i == arrows.Count) 
            {
                return IntIntervalRelation.Undefined;
            }

            return arrows[i].Key.RelationTo(bounds);
        }

        protected class BoundsRelation
        {
            // Arrow containing bounds
            public int ContainsIndex = -1;

            // First arrow overlapping with bounds
            public int ReplaceIndex = -1;

            // Arrow overlapping bounds.First
            public int FirstOverlapIndex = -1;

            // Arrows contained by bounds
            public readonly List<int> ContainedIndexes = new List<int>();

            // Arrow overlapping bounds.Last
            public int LastOverlapIndex = -1;
        }

        public IEnumerable<IntArrow<TAttr>> EnumerateCoverage(IntInterval bounds)
        {
            var relation = ParseBoundsRelation(bounds);
            if (relation.ContainsIndex >= 0)
            {
                yield return new IntArrow<TAttr>(bounds, arrows[relation.ContainsIndex].Value);
                yield break;
            }

            int prevIndex = bounds.First;

            if (relation.FirstOverlapIndex >= 0)
            {
                var arrow = arrows[relation.FirstOverlapIndex];
                var intersection = arrow.Key * bounds;
                yield return new IntArrow<TAttr>(intersection, arrow.Value);
                prevIndex = intersection.Last + 1;
            }

            foreach (int i in relation.ContainedIndexes)
            {
                var arrow = arrows[i];

                if (prevIndex < arrow.Key.First)
                {
                    yield return new IntArrow<TAttr>(prevIndex, arrow.Key.First - 1, DefaultValue);
                }
                
                yield return arrow;

                prevIndex = arrow.Key.Last + 1;
            }

            if (relation.LastOverlapIndex >= 0)
            {
                var arrow = arrows[relation.LastOverlapIndex];

                if (prevIndex < arrow.Key.First)
                {
                    yield return new IntArrow<TAttr>(prevIndex, arrow.Key.First - 1, DefaultValue);
                }

                var intersection = arrow.Key * bounds;
                yield return new IntArrow<TAttr>(intersection, arrow.Value);
            }
            else
            {
                if (prevIndex <= bounds.Last)
                {
                    yield return new IntArrow<TAttr>(prevIndex, bounds.Last, DefaultValue);
                }
            }
        }
    }
}
