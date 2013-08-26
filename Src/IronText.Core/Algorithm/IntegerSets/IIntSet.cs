
namespace IronText.Algorithm
{
    public interface IIntSet
    {
        MutableIntSet EditCopy();

        IntSet Clone();
        IntSet Complement();
        IntSet Complement(IntSet vocabulary);
        bool Contains(int value);
        int Count { get; }
        bool Equals(object obj);
        IntSet Except(IntSet other);
        int GetHashCode();
        IntSet Intersect(IntSet other);
        bool IsEmpty { get; }
        bool IsSubsetOf(IntSet other);
        bool IsSupersetOf(IntSet other);
        int Max();
        int Min();
        bool Overlaps(IntSet other);
        bool Rel(IntSet other, out IntSet onlyMine, out IntSet common, out IntSet onlyOthers);
        bool SetEquals(IntSet other);
        IntSet Union(IntSet other);

        System.Collections.Generic.IEnumerator<int> GetEnumerator();
    }
}
