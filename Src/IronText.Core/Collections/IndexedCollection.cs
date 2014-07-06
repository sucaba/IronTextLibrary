using IronText.Misc;
using System;
using System.Collections;
using System.Collections.Generic;

namespace IronText.Collections
{
    public class IndexedCollection<T, TScope> : IOwner<T>, IEnumerable<T>
        where T : class, IIndexable<TScope>, IHasIdentity
    {
        private const int InitialCapacity = 8;

        private T[] indexToItem;

        private readonly Dictionary<object,int> identityToIndex = new Dictionary<object,int>();
        private DuplicateResolution   _duplicateResolution;
        private IDuplicateResolver<T> _duplicateResolver;

        public IndexedCollection(TScope scope = default(TScope))
        {
            indexToItem = new T[InitialCapacity];
            this.Scope = scope;
            this.DuplicateResolution = DuplicateResolution.Fail;
        }

        public TScope Scope { get; private set; }

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
                else if (index >= Count)
                {
                    Count = index + 1;
                }
                else if (indexToItem[index] != null)
                {
                    DetachingItem(index);
                }

                AttachingItem(ref value);
                indexToItem[index] = value;
                AttachedItem(index);
            }
        }

        public DuplicateResolution DuplicateResolution
        {
            get {  return _duplicateResolution; }
            set
            {
                if (_duplicateResolution != value || _duplicateResolver == null)
                {
                    _duplicateResolution = value;
                    _duplicateResolver = CreateDuplicateResolver(_duplicateResolution);
                }
            }
        }

        private static IDuplicateResolver<T> CreateDuplicateResolver(DuplicateResolution resolution)
        {
            switch (resolution)
            {
                case DuplicateResolution.Fail:
                    return FailDuplicateResolver<T>.Instance;
                case DuplicateResolution.IgnoreNew:    
                    return IgnoreNewDuplicateResolver<T>.Instance;
                default:
                    throw new ArgumentException("resolution");
            }
        }

        public T Add(T item)
        {
            if (Contains(item))
            {
                throw new InvalidOperationException("IndexedCollection can contain only unique items.");
            }

            int index= AttachingItem(ref item);
            if (index < 0)
            {
                index = Count;
                if (indexToItem.Length == index)
                {
                    Array.Resize(ref indexToItem, index * 2);
                }
            }

            indexToItem[index] = item;
            AttachedItem(index);

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
                    item.Detaching(Scope);
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
                    item.Detaching(Scope);
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
                item.Detaching(Scope);
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
            if (!Contains(item))
            {
                Add(item);
            }
        }

        void IOwner<T>.Release(T item)
        {
            Remove(item);
        }

        private int AttachingItem(ref T newItem)
        {
            int existingIndex;
            if (newItem != null && identityToIndex.TryGetValue(newItem.Identity, out existingIndex))
            {
                var existing = indexToItem[existingIndex];
                var resolvedItem = _duplicateResolver.Resolve(existing, newItem);
                if (resolvedItem == existing)
                {
                    newItem = null;
                    return -1;
                }
                else
                {
                    DetachingItem(existingIndex);
                    newItem = resolvedItem;
                    return existingIndex;
                }
            }
          
            return -1;
        }

        private void AttachedItem(int index)
        {
            var item = indexToItem[index];
            if (item != null)
            {
                identityToIndex.Add(item.Identity, index);
                item.Attached(index, Scope);
            }
        }

        private void DetachingItem(int index)
        {
            indexToItem[index].Detaching(Scope);
            identityToIndex.Remove(indexToItem[index].Identity);
        }
    }
}