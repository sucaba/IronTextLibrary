using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Algorithm
{
    sealed class Buffer<T>
    {
        public T[] items;
        private int count; 

        public Buffer(int capacity)
        {
            this.items = new T[capacity];
        }

        public int Count { get { return count; } }

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
