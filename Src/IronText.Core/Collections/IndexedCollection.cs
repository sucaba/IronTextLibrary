using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework.Collections
{
    public class IndexedCollection<T, TContext> : IOwner<T>, IEnumerable<T>
        where T : class, IIndexable<TContext>
    {
        private const int InitialCapacity = 8;

        private T[] indexToItem;

        public IndexedCollection(TContext context = default(TContext))
        {
            indexToItem = new T[InitialCapacity];
            this.Context = context;
        }

        public TContext Context { get; private set; }

        public int Count { get; private set; }

        public T this[int index]
        {
            get { return indexToItem[index]; }
            set
            {
                if (index >= indexToItem.Length)
                {
                    Array.Resize(ref indexToItem, index + 1);
                    Count = index + 1;
                }
                else if (indexToItem[index] != null)
                {
                    indexToItem[index].Detach(Context);
                }

                indexToItem[index] = value;

                if (value != null)
                {
                    indexToItem[index].Attach(index, Context);
                }
            }
        }

        public T Add(T item)
        {
            int index = Count;
            if (indexToItem.Length == index)
            {
                Array.Resize(ref indexToItem, index * 2);
            }

            item.Attach(index, Context);
            indexToItem[index] = item;

            this.Count = index + 1;

            return item;
        }

        public void Clear()
        {
            int count = Count;
            var indexToItem = this.indexToItem;
            for (int i = 0; i != count; ++i)
            {
                var item = indexToItem[i];
                if (item != null)
                {
                    item.Detach(Context);
                    indexToItem[i] = null;
                }
            }

            Count = 0;
        }

        public bool Contains(T item)
        {
            int count = Count;
            var indexToItem = this.indexToItem;
            for (int i = 0; i != count; ++i)
            {
                if (indexToItem[i] == (object)item)
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(T[] array, int index)
        {
            int count = Count;
            var indexToItem = this.indexToItem;
            for (int i = 0; i != count; ++i)
            {
                var item = indexToItem[i];
                if (item != null)
                {
                    array[index++] = item;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            int count = Count;
            var indexToItem = this.indexToItem;
            for (int i = 0; i != count; ++i)
            {
                var item = indexToItem[i];
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        public bool Remove(T item)
        {
            if (item == null)
            {
                return false;
            }

            int count = Count;
            var indexToItem = this.indexToItem;

            for (int i = 0; i != count; ++i)
            {
                if (indexToItem[i] == (object)item)
                {
                    item.Detach(Context);
                    indexToItem[i] = null;
                    return true;
                }
            }

            return false;
        }

        public bool RemoveAt(int index)
        {
            var item = indexToItem[index];
            if (item != null)
            {
                item.Detach(Context);
                indexToItem[index] = null;
                return true;
            }

            return false;
        }

        public T[] ToArray()
        {
            var indexToItem = this.indexToItem;
            int count = Count;

            var result = new T[Count];
            int j = 0;
            for (int i = 0; i != count; ++i)
            {
                var item = indexToItem[i];
                if (item != null)
                {
                    result[j++] = item;
                }
            }

            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IOwner<T>.Acquire(T item)
        {
            Add(item);
        }

        void IOwner<T>.Release(T item)
        {
            Remove(item);
        }
    }
}