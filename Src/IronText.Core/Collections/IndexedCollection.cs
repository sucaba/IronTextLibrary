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

        private readonly IntAssoc intAssoc;
        private readonly List<T> items;

        private readonly Dictionary<object,int> identityToIndex = new Dictionary<object,int>();
        private IDuplicateResolver<T> _duplicateResolver;

        public IndexedCollection(TScope scope = default(TScope))
        {
            this.intAssoc = new IntAssoc(InitialCapacity);
            this.items = new List<T>(InitialCapacity);
            this.Scope = scope;
            this.DuplicateResolver = null;
        }

        public TScope Scope { get; private set; }

        public int IndexCount { get; private set; }

        public int PublicCount { get { return items.Count(IsPublicItem); }}

        /// <summary>
        /// Hidden items
        /// </summary>
        public IEnumerable<T> Hidden
        {
            get {  return items.Where(IsHiddenItem); }
        }

        /// <summary>
        /// All items including hidden
        /// </summary>
        public IEnumerable<T> All
        {
            get {  return items; }
        }

        public T this[int index]
        {
            get 
            { 
                var result = intAssoc.Get(index); 
                return IsPublicItem(result) ? result : null;
            }
            set
            {
                if (intAssoc.GrowToContain(index + 1) || index >= IndexCount)
                {
                    IndexCount = index + 1;
                }
                else if (intAssoc.indexToItem[index] != null)
                {
                    DetachingItem(index);
                }

                AttachingItem(ref value);
                intAssoc.indexToItem[index] = value;
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
                if (intAssoc.indexToItem.Length == index)
                {
                    Array.Resize(ref intAssoc.indexToItem, index * 2);
                }
            }

            intAssoc.indexToItem[index] = item;
            AttachedItem(index);

            this.IndexCount = index + 1;

            return item;
        }

        public bool Contains(T item)
        {
            int count = IndexCount;
            for (int i = 0; i != count; ++i)
            {
                if (intAssoc.indexToItem[i] == (object)item)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int count = IndexCount;
            for (int i = 0; i != count; ++i)
            {
                var item = intAssoc.indexToItem[i];
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

            for (int i = 0; i != count; ++i)
            {
                if (intAssoc.indexToItem[i] == (object)item)
                {
                    item.Detaching(Scope);
                    intAssoc.indexToItem[i] = null;
                    return true;
                }
            }

            return false;
        }

        public T[] ToArray()
        {
            int count = IndexCount;

            var result = new T[IndexCount];
            int j = 0;
            for (int i = 0; i != count; ++i)
            {
                var item = intAssoc.indexToItem[i];
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
                var existing = intAssoc.indexToItem[existingIndex];
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
            var item = intAssoc.indexToItem[index];
            if (item != null)
            {
                items.Add(item);
                identityToIndex.Add(item.Identity, index);
                item.Attached(index, Scope);
            }
        }

        private void DetachingItem(int index)
        {
            var item = intAssoc.indexToItem[index];
            item.Detaching(Scope);
            identityToIndex.Remove(item.Identity);
            items.Remove(item);
        }

        private static bool IsHiddenItem(T item)
        {
            return item != null && item.IsHidden;
        }

        // TODO: Make public indexed and hidden non-indexed. 
        // This way code will be simple and fast.
        private static bool IsPublicItem(T item)
        {
            return item != null && !item.IsHidden;
        }

        class IntAssoc
        {
            internal T[] indexToItem;

            public IntAssoc(int capacity)
            {
                this.indexToItem = new T[capacity];
            }

            public T Get(int index)
            {
                return indexToItem[index];
            }

            public bool GrowToContain(int count)
            {
                if (count > indexToItem.Length)
                {
                    Array.Resize(ref indexToItem, count);
                    return true;
                }

                return false;
            }
        }
    }

}