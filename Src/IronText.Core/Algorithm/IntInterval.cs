using System;

namespace IronText.Algorithm
{
    using Int = System.Int32;

    [Flags]
    public enum IntIntervalPosition : byte
    {
        Undefined       = 0x00,

        Less            = 0x01,
        First           = 0x02,
        StrictlyInside  = 0x04,
        Last            = 0x08,
        Greater         = 0x10,

        InsideMask      = First | StrictlyInside | Last,
        OutsideMask     = Less | Greater,
        EdgeMask        = First | Last,
    }

    [Flags]
    public enum IntIntervalRelation
    {
        Undefined       = 0x00,

        Less            = 0x01,
        OverlapLast     = 0x04,
        Contained       = 0x02,
        OverlapFirst    = 0x08,
        Contains        = 0x10,
        Greater         = 0x20,
        Equal           = Contained | Contains,

        OverlapMask     = Contained       
                        | Contains
                        | OverlapLast
                        | OverlapFirst
    }

    public struct IntInterval
    {
        public readonly Int First;
        public readonly Int Last; // inclusive
        public static readonly IntInterval Empty = new IntInterval(int.MaxValue, int.MinValue);

        public IntInterval(Int first)
        {
            First = first;
            Last = first;
        }

        public IntInterval(Int first, Int last)
        {
            First = first;
            Last = last;
        }

        public bool IsEmpty { get { return Last < First; } }

        public int Size { get { checked { return Last - First + 1; } } }

        public long LongSize { get { checked { return Last - (long)First + 1; } } }

        public IntIntervalPosition PositionOf(Int value)
        {
            checked
            {
                if (IsEmpty)
                {
                    return IntIntervalPosition.Undefined;
                }

                long firstDiff = (long)value - First;
                if (firstDiff < 0)
                {
                    return IntIntervalPosition.Less;
                }
                if (firstDiff == 0)
                {
                    return IntIntervalPosition.First;
                }
                long lastDiff = (long)value - Last;
                if (lastDiff < 0)
                {
                    return IntIntervalPosition.StrictlyInside;
                }
                if (lastDiff == 0)
                {
                    return IntIntervalPosition.Last;
                }

                return IntIntervalPosition.Greater;
            }
        }

        public bool Contains(Int actual) { return actual >= First && actual <= Last; }

        public bool Contains(IntInterval other) { return other.First >= First && other.Last <= Last; }

        public static bool operator ==(IntInterval x, IntInterval y) { return x.First == y.First && x.Last == y.Last; }
        public static bool operator !=(IntInterval x, IntInterval y) { return !(x == y); }

        public static bool operator <(IntInterval x, IntInterval y) { return x.Last < y.First; }
        public static bool operator >(IntInterval x, IntInterval y) { return y.Last < x.First; }

        public override bool Equals(object obj)
        {
            return this == (IntInterval)obj;
        }

        public override Int GetHashCode()
        {
            unchecked
            {
                return First + Last;
            }
        }

        /// <summary>
        /// Intersection
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IntInterval operator *(IntInterval x, IntInterval y)
        {
            var result = new IntInterval(
                (x.First > y.First) ? x.First : y.First,
                 (x.Last < y.Last) ? x.Last : y.Last);
            return result;
        }

        public IntInterval LeftCut(Int inclusiveLimit)
        {
            return new IntInterval(First, inclusiveLimit);
        }

        public IntInterval RigthCut(Int inclusiveLimit)
        {
            return new IntInterval(inclusiveLimit, Last);
        }

        public static IntInterval operator -(IntInterval x, Int value)
        {
            int first, last;
            if (x.First == value)
            {
                first = value + 1;
            }
            else
            {
                first = x.First;
            }

            if (x.Last == value)
            {
                last = value - 1;
            }
            else
            {
                last = x.Last;
            }

            return new IntInterval(first, last);
        }

        public IntInterval Union(IntInterval other)
        {
            var result = new IntInterval(
                Math.Min(First, other.First),
                Math.Max(Last, other.Last));
            return result;
        }

        public bool IsNextTo(IntInterval newInterval)
        {
            bool newIsLefter = newInterval.Last + 1 < First;
            bool newIsRighter = newInterval.First > Last + 1;
            return !(newIsLefter || newIsRighter);
        }

        public bool Intersects(IntInterval other) { return !(this < other || other < this); }

        public bool Disjoined(IntInterval other) { return this < other  || other < this; }

        public IntIntervalRelation RelationTo(IntInterval other)
        {
            if (this < other)
            {
                return IntIntervalRelation.Less;
            }
            else if (other < this)
            {
                return IntIntervalRelation.Greater;
            }
            else if (this == other)
            {
                return IntIntervalRelation.Equal;
            }
            else if (this.Contains(other))
            {
                return IntIntervalRelation.Contains;
            }
            else if (other.Contains(this))
            {
                return IntIntervalRelation.Contained;
            }
            else if (this.Contains(other.First))
            {
                return IntIntervalRelation.OverlapFirst;
            }
            else if (other.Contains(this.First))
            {
                return IntIntervalRelation.OverlapLast;
            }
            else
            {
                throw new InvalidOperationException("Internal error");
            }
        }

        public override string ToString()
        {
            if (First == Last)
            {
                return string.Format("0x{0:X}", First);
            }

            return string.Format("0x{0:X}..0x{1:X}", First, Last);
        }

        public string ToCharSetString()
        {
            if (First == Last)
            {
                return NameOfChar(First);
            }

            return string.Format("{0}..{1}", NameOfChar(First), NameOfChar(Last));
        }

        private static string NameOfChar(int ch)
        {
            if (ch == '\n') { return "\\n"; }
            if (ch == '\r') { return "\\r"; }
            if (ch == '\t') { return "\\t"; }
            if (ch == '\b') { return "\\b"; }
            if (ch == ' ') { return "<SPACE>"; }

            if (char.IsControl((char)ch) || ch >= 0xffff)
            {
                return string.Format("0x{0:X}", ch);
            }
            else
            {
                return ((char)ch).ToString();
            }
        }

        public IntInterval Before(IntInterval bounds)
        {
            if (Last < bounds.First)
            {
                return this;
            }

            // Maybe empty
            return new IntInterval(First, bounds.First - 1);
        }

        public IntInterval After(IntInterval bounds)
        {
            if (bounds.Last < First)
            {
                return this;
            }

            // Maybe empty
            return new IntInterval(bounds.Last + 1, Last);
        }
    }
}
