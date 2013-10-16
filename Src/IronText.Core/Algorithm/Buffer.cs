using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Algorithm
{
    public sealed class Buffer<T>
    {
        public T[] items;
        private int count; 

        public Buffer(int capacity)
        {
            this.items = new T[capacity];
        }

        public int Count { get { return count; } }

        public bool IsEmpty { get { return count == 0; } }

        public bool HasItems { get { return count != 0; } }

        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i != count; ++i)
            {
                action(items[i]);
            }
        }

        public void Add(T value)
        {
            if (count == items.Length)
            {
                Array.Resize(ref items, count * 2);
            }

            items[count++] = value;
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
                items[i] = default(T);
            }

            count = 0;
        }
    }
}
