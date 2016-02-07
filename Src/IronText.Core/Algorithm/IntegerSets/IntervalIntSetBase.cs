using System;
using System.Collections.Generic;
using System.Linq;
using Int = System.Int32;

namespace IronText.Algorithm
{
    [Serializable]
    public abstract class IntervalIntSetBase : MutableIntSet
    {
        // Global bounds for performance
        internal IntInterval bounds;

        // Sorted disjoined intervals
        internal List<IntInterval> intervals;

        /// <summary>
        /// Unsafe constructor for constructing from the ready intervals list
        /// </summary>
        /// <param name="setType"></param>
        /// <param name="intervals">sorted intervals list or <c>null</c></param>
        protected IntervalIntSetBase(IntSetType setType)
            : base(setType)
        {
            this.intervals = new List<IntInterval>();
            UpdateHashAndBounds();
        }

        public override bool IsEmpty { get { return intervals.Count == 0; } }

        public override Int Count 
        {
            get
            {
                Int result = 0;

                foreach (var interval in intervals)
                {
                    result += (interval.Last - interval.First + 1);
                }

                return result;
            }
        }

        public override IntSet Clone() { return TypedClone(); }

        public override bool SetEquals(IntSet other0)
        {
            var other = (IntervalIntSetBase)other0;
            if (hash != other.hash) return false;
            if (bounds != other.bounds || intervals.Count != other.intervals.Count)
            {
                return false;
            }

            int count = intervals.Count;
            for (int i = 0; i != count; ++i)
            {
                if (intervals[i] != other.intervals[i])
                {
                    return false;
                }
            }

            return true; 
        }
        
        public override bool Contains(Int actual)
        {
            if (bounds.Contains(actual))
            {
                foreach (var interval in intervals)
                {
                    if (actual >= interval.First && actual <= interval.Last)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override IntSet Intersect(IntSet other0)
        {
            IntervalIntSetBase other = (IntervalIntSetBase)other0;

            if (bounds.Intersects(other.bounds))
            {
                var result = new IntervalIntSet(setType);

                var otherIntervals = other.intervals;
                int myCount = intervals.Count;
                int theirCount = otherIntervals.Count;
                int i = 0, j = 0;

                while (i != myCount && j != theirCount)
                {
                    IntInterval mine = intervals[i];
                    IntInterval theirs = otherIntervals[j];

                    switch (mine.RelationTo(theirs))
                    {
                        case IntIntervalRelation.Less:
                        {
                            ++i;
                            break;
                        }
                        case IntIntervalRelation.Greater:
                        {
                            ++j;
                            break;
                        }
                        case IntIntervalRelation.Equal:
                        case IntIntervalRelation.Contains:
                        {
                            result.Add(theirs);
                            ++j;
                            break;
                        }
                        case IntIntervalRelation.Contained:
                        {
                            result.Add(mine);
                            ++i;
                            break;
                        }
                        case IntIntervalRelation.OverlapFirst:
                        {
                            result.Add(mine * theirs);
                            ++i;
                            break;
                        }
                        case IntIntervalRelation.OverlapLast:
                        {
                            result.Add(mine * theirs);
                            ++j;
                            break;
                        }
                        default:
                            throw new InvalidOperationException("Internal error");
                    }
                }

                result.UpdateHash();
                return result;
            }

            return setType.Empty;
        }

        public override IntSet Union(IntSet other0)
        {
            var result = TypedClone();
            result.AddAll(this);
            result.AddAll(other0);
            result.UpdateHash();
            return result;
        }

        public override IntSet Complement(IntSet vocabulary0)
        {
            var vocabulary = vocabulary0 as IntervalIntSetBase;
            if (vocabulary == null)
            {
                throw new ArgumentException("Unsupported set type:" + vocabulary0, "vocabulary");
            }

            var result = new IntervalIntSet(setType);
            Int previous = setType.MinValue;
            foreach (var interval in intervals)
            {
                if (interval.First != previous)
                {
                    var set = setType.Range(previous, interval.First - 1);
                    result.AddAll(set.Intersect(vocabulary));
                }

                previous = interval.Last + 1;
            }

            if (previous != setType.MaxValue)
            {
                var set = setType.Range(previous, setType.MaxValue);
                result.AddAll(set.Intersect(vocabulary));
            }

            result.UpdateHash();
            return result;
        }

        public override void AddAll(IntSet other)
        {
            var otherIntervalSet = (IntervalIntSetBase)other;
            foreach (var interval in otherIntervalSet.intervals)
            {
                Add(interval);
            }

            UpdateHash();
        }

        public override void RemoveAll(IntSet other)
        {
            var complemented = (IntervalIntSetBase)other.Complement(this);
            this.intervals = complemented.intervals;
            this.bounds = complemented.bounds;
            this.hash = complemented.hash;
        }

        public override void Add(Int value)
        {
            Add(new IntInterval(value, value));
        }

        public override Int PopAny()
        {
            if (intervals.Count == 0)
            {
                throw new InvalidOperationException("Unable to pop element from the empty set");
            }

            int index = intervals.Count - 1;
            var interval = intervals[index];
            if (interval.First == interval.Last)
            {
                intervals.RemoveAt(index);
                return interval.First;
            }

            Int result = interval.Last;
            intervals[index] = new IntInterval(interval.First, interval.Last - 1);
            return result;
        }

        public override void Remove(Int value)
        {
            if (bounds.Contains(value))
            {
                int i = FindInterval(value);
                if (i >= 0)
                {
                    var interval = intervals[i];
                    if (interval.First == interval.Last)
                    {
                        intervals.RemoveAt(i);
                    }
                    else if (interval.First == value)
                    {
                        intervals[i] = new IntInterval(value + 1, interval.Last);
                    }
                    else if (interval.Last == value)
                    {
                        intervals[i] = new IntInterval(interval.First, value - 1);
                    }
                    else
                    {
                        intervals[i] = new IntInterval(value + 1, interval.Last);
                        intervals.Insert(i, new IntInterval(interval.First, value - 1));
                    }

                    bounds = bounds - value;
                }
            }
        }

#if false
        // TODO: Performance in set differences
        private void Remove(IntInterval otherInterval)
        {
            // TODO: BOUNDS update at the end
            if (bounds.Intersects(otherInterval))
            {
                bounds = IntInterval.Empty;

                bool notDone = false;
                for (int i = 0; i != intervals.Count && notDone; ++i)
                {
                    var interval = intervals[i];
                    var rel = interval.RelationTo(otherInterval);
                    switch (rel)
                    {
                        case IntIntervalRelation.Contained:
                            intervals.RemoveAt(i);
                            continue;
                        case IntIntervalRelation.Contains:
                            intervals[i] = interval.Before(otherInterval);
                            intervals.Insert(i + 1, interval.After(otherInterval));
                            return;
                        case IntIntervalRelation.OverlapFirst:
                            intervals[i] = interval.Before(otherInterval);
                            ++i;
                            continue;
                        case IntIntervalRelation.OverlapLast:
                            intervals[i] = interval.After(otherInterval);
                            return;
                        default:
                            ++i;
                            continue;
                    }
                }
            }
        }
#endif

        public override void Add(IntInterval newInterval)
        {
            if (newInterval.First > newInterval.Last)
            {
                return;
            }

            bool changed = false;
            var oldBounds = bounds;
            bounds = bounds.Union(newInterval);

            if (oldBounds.IsNextTo(newInterval))
            {
                bool done = false;
                for (int i = 0; i != intervals.Count; ++i)
                {
                    IntInterval interval = intervals[i];
                    if (interval == newInterval)
                    {
                        // already in set
                        done = true;
                        break;
                    }

                    if (interval.IsNextTo(newInterval))
                    {
                        changed = !interval.Contains(newInterval);

                        var bigger = interval.Union(newInterval);
                        intervals[i] = bigger;

                        for (int j = i + 1; j != intervals.Count; )
                        {
                            interval = intervals[j];
                            if (bigger.IsNextTo(interval))
                            {
                                bigger = bigger.Union(interval);
                                intervals[i] = bigger;
                                intervals.RemoveAt(j);
                            }
                            else
                            {
                                ++j;
                            }
                        }

                        done = true;
                        break;
                    }

                    if (newInterval < interval)
                    {
                        intervals.Insert(i, newInterval);
                        done = true;
                        changed = true;
                        break;
                    }
                }

                if (!done)
                {
                    intervals.Insert(0, newInterval);
                    changed = true;
                }
            }
            else if (newInterval < oldBounds)
            {
                intervals.Insert(0, newInterval);
                changed = true;
            }
            else
            {
                intervals.Add(newInterval);
                changed = true;
            }

            if (changed)
            {
                UpdateHash();
            }
        }

        public override IEnumerator<Int> GetEnumerator() { return All().GetEnumerator(); }

        public override string ToString() 
        { 
            return "{" + string.Join(", ", intervals) + "}"; 
        }

        public override string ToCharSetString() 
        { 
            return "{" + string.Join(", ", intervals.Select(interv => interv.ToCharSetString())) + "}"; 
        }

        public override Int Min()
        {
            if (intervals.Count == 0)
            {
                throw new InvalidOperationException("Source is empty");
            }

            return intervals[0].First;
        }

        public override Int Max()
        {
            int count = intervals.Count;
            if (count == 0)
            {
                throw new InvalidOperationException("Source is empty");
            }

            return intervals[count - 1].Last;
        }

        internal IntervalIntSet TypedClone()
        {
            var result = new IntervalIntSet(setType);
            foreach (var item in intervals)
            {
                result.intervals.Add(item);
            }

            result.bounds = bounds;
            return result;
        }

        private IEnumerable<Int> All()
        {
            foreach (var interval in intervals)
            {
                for (Int i = interval.First; i <= interval.Last; ++i)
                {
                    yield return i;
                    if (i == setType.MaxValue)
                    {
                        break;
                    }
                }
            }
        }

        private Int FindInterval(Int value)
        {
            for (int i = 0; i != intervals.Count; ++i)
            {
                if (intervals[i].Contains(value))
                {
                    return i;
                }
            }

            return -1;
        }

        protected void UpdateHashAndBounds()
        {
            bounds = new IntInterval(setType.MaxValue, setType.MinValue);
            foreach (var interval in intervals)
            {
                bounds = bounds.Union(interval);
            }

            UpdateHash();
        }

        private void UpdateHash()
        {
            this.hash = (int)(this.bounds.First << 16) | (this.intervals.Count & 0x0000ffff);
        }

        public override MutableIntSet EditCopy()
        {
            var result = new MutableIntervalIntSet(setType);
            result.AddAll(this);
            return result;
        }

        public override IEnumerable<IntInterval> EnumerateIntervals() { return intervals; }
    }
}
