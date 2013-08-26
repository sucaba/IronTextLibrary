using System.Collections.Generic;

namespace IronText.Algorithm
{
    public struct ArraySlice<T> : IEnumerable<T>
    {
        public readonly T[] Array;
        public readonly int Offset;
        public readonly int Count;

        public ArraySlice(T[] array, int offset, int count)
        {
            this.Array  = array;
            this.Offset = offset;
            this.Count  = count;
        }

        public ArraySlice(T[] array, int offset)
        {
            this.Array  = array;
            this.Offset = offset;
            this.Count  = array.Length - offset;
        }

        public ArraySlice(T[] array)
        {
            this.Array  = array;
            this.Offset = 0;
            this.Count  = array.Length;
        }

        public T this[int index]
        {
            get { return Array[Offset + index]; }
            set { Array[Offset + index] = value; }
        }

        public ArraySlice<T> SubSlice(int relativeStart)
        {
            int start = Offset + relativeStart;
            return new ArraySlice<T>(Array, start, Count - relativeStart);
        }

        public ArraySlice<T> SubSlice(int relativeStart, int count)
        {
            return new ArraySlice<T>(Array, Offset + relativeStart, count);
        }

        public T[] ToArray()
        {
            var result = new T[Count];
            int i = Offset;
            int end = Offset + Count;
            for (; i != end; ++i)
            {
                result[i - Offset] = Array[i];
            }
            
            return result;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            int i = Offset;
            int end = Offset + Count;
            for (; i != end; ++i)
            {
                yield return Array[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        public void CopyTo(T[] array, int destIndex)
        {
            int end = Offset + Count;
            for (int i = Offset; i != end; ++i)
            {
                array[destIndex + i - Offset] = Array[i];
            }
        }
    }
}
