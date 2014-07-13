using IronText.Misc;
using System;
using System.Linq;
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
        private IDuplicateResolver<T> _duplicateResolver;

        public IndexedCollection(TScope scope = default(TScope))
        {
            this.indexToItem = new T[InitialCapacity];
            this.Scope = scope;
            this.DuplicateResolver = null;
        }

        public TScope Scope { get; private set; }

        public int IndexCount { get; private set; }

        public int PublicCount { get { return indexToItem.Count(IsPublicItem); }}

        /// <summary>
        /// Hidden items
        /// </summary>
        public IEnumerable<T> Hidden
        {
            get {  return indexToItem.Where(IsHiddenItem); }
        }

        /// <summary>
        /// All items including hidden
        /// </summary>
        public IEnumerable<T> All
        {
            get {  return indexToItem.Where(x => x != null); }
        }

        public T this[int index]
        {
            get 
            { 
                var result = indexToItem[index]; 
                return IsPublicItem(result) ? result : null;
            }
            set
            {
                if (index >= indexToItem.Length)
                {
                    Array.Resize(ref indexToItem, index + 1);
                    IndexCount = index + 1;
                }
                else if (index >= IndexCount)
                {
                    IndexCount = index + 1;
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

        public IDuplicateResolver<T> DuplicateResolver
        {
            get {  return _duplicateResolver; }
            set { _duplicateResolver = value ?? DuplicateResolver<T>.Fail; }
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
                index = IndexCount;
                if (indexToItem.Length == index)
                {
                    Array.Resize(ref indexToItem, index * 2);
                }
            }

            indexToItem[index] = item;
            AttachedItem(index);

            this.IndexCount = index + 1;

            return item;
        }

        public void Clear()
        {
            int count = IndexCount;
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

            IndexCount = 0;
        }

        public bool Contains(T item)
        {
            int count = IndexCount;
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
            int count = IndexCount;
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
            return GetEnumerator(false);
        }

        private IEnumerator<T> GetEnumerator(bool includeHidden)
        {
            int count = IndexCount;
            var indexToItem = this.indexToItem;
            for (int i = 0; i != count; ++i)
            {
                var item = indexToItem[i];
                if (IsPublicItem(item))
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

            int count = IndexCount;
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
            int count = IndexCount;

            var result = new T[IndexCount];
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

        private static bool IsHiddenItem(T item)
        {
            return item != null && item.IsHidden;
        }

        private static bool IsPublicItem(T item)
        {
            return item != null && !item.IsHidden;
        }
    }
}