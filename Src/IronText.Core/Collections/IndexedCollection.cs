using IronText.Misc;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace IronText.Collections
{
    [Serializable]
    public class IndexedCollection<T, TScope> : IOwner<T>, IEnumerable<T>
        where T : class, IIndexable<TScope>, IHasIdentity
    {
        private const int InitialCapacity = 8;

        private readonly IntAssoc intAssoc;
        private readonly List<T> items;

        private readonly Dictionary<object,int> identityToIndex = new Dictionary<object,int>();
        [NonSerialized]
        private IDuplicateResolver<T> _duplicateResolver;
        private bool indexed;
        private bool canModify = true;

        public IndexedCollection(TScope scope = default(TScope))
        {
            this.intAssoc = new IntAssoc(InitialCapacity);
            this.items = new List<T>(InitialCapacity);
            this.Scope = scope;
            this.DuplicateResolver = null;
        }

        public void BuildIndexes()
        {
            this.indexed   = true;
            this.canModify = false;
        }

        private void RequireIndexed()
        {
            if (!indexed)
            {
                 throw new InvalidOperationException();
            }
        }

        private void RequireModifiable()
        {
            if (!canModify)
            {
                 // throw new InvalidOperationException();
            }
        }


        public TScope Scope { get; private set; }

        public int IndexCount { get { RequireIndexed(); return intAssoc.Count; } }

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
                RequireIndexed();
                var result = intAssoc.Get(index); 
                return IsPublicItem(result) ? result : null;
            }
        }

        public IDuplicateResolver<T> DuplicateResolver
        {
            get {  return _duplicateResolver; }
            set { _duplicateResolver = value ?? DuplicateResolver<T>.Fail; }
        }

        public T Add(T item)
        {
            RequireModifiable();

            if (Contains(item))
            {
                throw new InvalidOperationException("IndexedCollection can contain only unique items.");
            }

            int index = AttachingItem(ref item);
            if (index < 0)
            {
                index = intAssoc.GenerateIndex();
            }

            intAssoc.Set(index, item);
            AttachedItem(index);

            return item;
        }

        public bool Contains(T item)
        {
            return items.IndexOf(item) >= 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.Where(IsPublicItem).GetEnumerator();
        }

        public bool Remove(T item)
        {
            RequireModifiable();

            if (item == null)
            {
                return false;
            }

            int i = intAssoc.IndexOf(item);
            if (i < 0)
            {
                return false;
            }

            item.Detaching(Scope);
            items.Remove(item);
            intAssoc.Set(i, null);
            return true;
        }

        public T[] ToArray()
        {
            RequireIndexed();
            return intAssoc.ToArray();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IOwner<T>.Acquire(T item)
        {
            RequireModifiable();

            if (!Contains(item))
            {
                Add(item);
            }
        }

        void IOwner<T>.Release(T item)
        {
            RequireModifiable();

            Remove(item);
        }

        private int AttachingItem(ref T newItem)
        {
            int existingIndex;
            if (newItem != null && identityToIndex.TryGetValue(newItem.Identity, out existingIndex))
            {
                var existing = intAssoc.Get(existingIndex);
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
            var item = intAssoc.Get(index);
            if (item != null)
            {
                items.Add(item);
                identityToIndex.Add(item.Identity, index);
                item.Attached(index, Scope);
            }
        }

        private void DetachingItem(int index)
        {
            var item = intAssoc.Get(index);
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

        [Serializable]
        class IntAssoc
        {
            private T[] indexToItem;

            public IntAssoc(int capacity)
            {
                this.indexToItem = new T[capacity];
            }

            public int Count { get; private set; }

            public T Get(int index)
            {
                return indexToItem[index];
            }

            public void Set(int index, T value)
            {
                indexToItem[index] = value;
            }

            public int IndexOf(T item)
            {
                int count = Count;
                for (int i = 0; i != count; ++i)
                {
                    if (Get(i) == (object)item)
                    {
                        return i;
                    }
                }

                return -1;
            }

            public int GenerateIndex()
            {
                int result = Count;
                if (indexToItem.Length == result)
                {
                    Array.Resize(ref indexToItem, result * 2);
                }
            
                Count = result + 1;

                return result;
            }

            public bool Grow(int count)
            {
                if (count > indexToItem.Length)
                {
                    Array.Resize(ref indexToItem, count);
                    Count = count;
                    return true;
                }
                else if (count > Count)
                {
                    Count = count;
                    return true;
                }

                return false;
            }

            public T[] ToArray()
            {
                int count = Count;

                var result = new T[Count];
                int j = 0;
                for (int i = 0; i != count; ++i)
                {
                    var item = Get(i);
                    if (item != null)
                    {
                        result[j++] = item;
                    }
                }

                return result;
            }
        }
    }

}