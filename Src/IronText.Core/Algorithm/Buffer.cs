using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Algorithm
{
    sealed class Buffer<T>
    {
        private T[] data;
        private int count; 

        public Buffer(int count)
        {
            this.data = new T[count];
        }

        public T[] Data { get { return data; } }

        public int Count 
        { 
            get { return count; }
            set
            {
                if (data.Length != value)
                {
                    Array.Resize(ref data, value);
                }
            }
        }

        public void Ensure(int minCount)
        {
            if (data.Length < minCount)
            {
                Array.Resize(ref data, minCount);
            }
        }

        public bool IsEmpty { get { return count == 0; } }

        public bool HasItems { get { return count != 0; } }

        public T this[int index]
        {
            get { return data[index]; }
            set{ data[index] = value; }
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i != count; ++i)
            {
                action(data[i]);
            }
        }

        public void Add(T value)
        {
            if (Count == data.Length)
            {
                Count = Count * 2;
            }

            data[count++] = value;
        }

        public void AddRange(Buffer<T> other)
        {
            int otherCount = other.Count;
            for (int i = 0; i != otherCount; ++i)
            {
                Add(other[i]);
            }
        }

        public void Clear()
        {
            for (int i = 0; i != count; ++i)
            {
                data[i] = default(T);
            }

            count = 0;
        }

        public void Fill(IList<T> items)
        {
            Ensure(items.Count);
            Fill((IEnumerable<T>)items);
        }

        public void Fill(IEnumerable<T> items) 
        {
            int i = 0;
            foreach (var item in items)
            {
                data[i++] = item;
            }
        }
    }
}
