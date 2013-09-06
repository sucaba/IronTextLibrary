
using Int = System.Int32;

namespace IronText.Algorithm
{
    public struct IntArrow<T>
    {
        public readonly IntInterval Key;
        public readonly T           Value;

        public IntArrow(IntInterval key, T value)
        {
            this.Key = key;
            this.Value = value;
        }

        public IntArrow(Int key, T value)
        {
            this.Key = new IntInterval(key);
            this.Value = value;
        }

        public IntArrow(Int first, Int last, T value)
        {
            this.Key = new IntInterval(first, last);
            this.Value = value;
        }

        public bool IsEmpty { get { return Key.IsEmpty; } }

        public IntArrow<T> Before(IntInterval bounds)
        {
            return new IntArrow<T>(Key.Before(bounds), Value);
        }

        public IntArrow<T> After(IntInterval bounds)
        {
            return new IntArrow<T>(Key.After(bounds), Value);
        }

        public static IntArrow<T> operator*(IntArrow<T> arrow, IntInterval bounds)
        {
            return new IntArrow<T>(arrow.Key * bounds, arrow.Value);
        }

        public override string ToString()
        {
            return string.Format("{0} --> {1}", Key, Value);
        }
    }
}
