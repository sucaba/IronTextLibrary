using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Int = System.Int32;

namespace IronText.Algorithm
{
    [Serializable]
    public abstract class IntSet : IEnumerable<Int>, IIntSet
    {
        public static IntSet Clone(IntSet set) { return set.Clone(); }

        protected internal readonly IntSetType setType;

        // (firstElement or 0) << 16 | (intervalCount & 0x0000ffff) 
        internal int hash;

        protected IntSet(IntSetType setType) { this.setType = setType; }

        public IntSetType SetType { get { return setType; } }

        public bool IsSupersetOf(IntSet other) { return other.IsSubsetOf(this); }
        public IntSet Complement() { return Complement(setType.All); }
        public IntSet Except(IntSet other) { return Intersect(other.Complement(setType.All)); }
        public bool Overlaps(IntSet other) { return !Intersect(other).IsEmpty; }
        public bool IsSubsetOf(IntSet other) { return Intersect(other).SetEquals(this); }

        public bool Rel(IntSet other, out IntSet onlyMine, out IntSet common, out IntSet onlyOthers)
        {
            common = Intersect(other);
            onlyMine = Except(common);
            onlyOthers = other.Except(common);
            return !common.IsEmpty;
        }

        public abstract bool Contains(Int value);
        public abstract bool IsEmpty { get; }
        public abstract Int Count { get; }
        public abstract IntSet Union(IntSet other);
        public abstract IntSet Intersect(IntSet other);
        public abstract IntSet Complement(IntSet vocabulary);
        public abstract bool SetEquals(IntSet other);
        public abstract IntSet Clone();

        public abstract Int Min();
        public abstract Int Max();

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        public abstract IEnumerator<Int> GetEnumerator();

        public override bool Equals(object obj)
        {
            var casted = obj as IntSet;
            return casted != null && SetEquals(casted);
        }

        public override int GetHashCode() { return hash; }

        public abstract MutableIntSet EditCopy();

        public abstract IEnumerable<IntInterval> EnumerateIntervals();

        public override string ToString()
        {
            var output = new StringBuilder();;
            output.Append("{");
            bool first = true;
            int limitCountdown = 20;
            foreach (var item in this)
            {
                if (limitCountdown-- == 0)
                {
                    output.Append(" ...");
                    break;
                }

                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Append(", ");
                }

                output.Append(item);
            }

            output.Append("}");
            return output.ToString();
        }

        public abstract string ToCharSetString();
    }
}
