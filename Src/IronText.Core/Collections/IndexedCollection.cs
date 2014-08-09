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

        private readonly List<T> items;

        [NonSerialized]
        private IDuplicateResolver<T> _duplicateResolver;
        private bool indexed   = false;
        private bool canModify = true;
        private T[] indexes;
        private readonly Dictionary<object,T> identityToItem = new Dictionary<object,T>();
        private readonly Dictionary<int, T> forcedIndexes = new Dictionary<int,T>();

        public IndexedCollection(TScope scope = default(TScope))
        {
            this.items = new List<T>(InitialCapacity);
            this.Scope = scope;
            this.DuplicateResolver = null;
        }

        public void BuildIndexes(int startIndex = 0)
        {
            this.StartIndex = startIndex;
            this.indexed   = true;
            this.canModify = false;

            int lastForcedIndex = forcedIndexes.Count == 0 ? 0 : forcedIndexes.Keys.Max() + 1;
            int lastIndex = Math.Max(startIndex + items.Count(IsPublicItem), lastForcedIndex);
            this.indexes = new T[lastIndex];

            foreach (var pair in forcedIndexes)
            {
                if (pair.Key < startIndex)
                {
                    throw new InvalidOperationException("Forced index should be lower than a start index.");
                }

                indexes[pair.Key] = pair.Value;
                pair.Value.AssignIndex(pair.Key);
            }

            var itemsToIndex = items.Except(forcedIndexes.Values, ReferenceComparer<T>.Default);
            int count = indexes.Length;
            int i = startIndex;
            foreach (var item in itemsToIndex)
            {
                if (IsPublicItem(item))
                {
                    while (forcedIndexes.ContainsKey(i))
                    {
                        ++i;
                    }

                    indexes[i] = item;
                    item.AssignIndex(i);
                    ++i;
                }
            }
        }

        private bool HasForcedIndex(T item)
        {
            return forcedIndexes.Values.Contains(item);
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
                 throw new InvalidOperationException();
            }
        }

        public TScope  Scope       { get; private set; }

        /// <summary>
        /// Index of the first indexed element
        /// </summary>
        public int     StartIndex  { get; private set; }

        /// <summary>
        /// Index of element following the last one
        /// </summary>
        public int     LastIndex   { get { RequireIndexed(); return indexes.Length; } }

        public int     PublicCount { get { return items.Count(IsPublicItem); }}

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
                return indexes[index];
            }
        }

        public IDuplicateResolver<T> DuplicateResolver
        {
            get {  return _duplicateResolver; }
            set { _duplicateResolver = value ?? DuplicateResolver<T>.Fail; }
        }

        public T Add(T item, int forcedIndex = -1)
        {
            RequireModifiable();

            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (Contains(item))
            {
                throw new InvalidOperationException("IndexedCollection can contain only unique items.");
            }

            if (!AttachingItem(ref item))
            {
                return item;
            }

            AttachedItem(item);

            if (forcedIndex >= 0)
            {
                forcedIndexes.Add(forcedIndex, item);
            }

            return item;
        }

        private bool AttachingItem(ref T newItem)
        {
            T existing;
            if (identityToItem.TryGetValue(newItem.Identity, out existing))
            {
                var resolved = _duplicateResolver.Resolve(existing, newItem);
                if (resolved == existing)
                {
                    newItem = existing;
                    return false;
                }
                
                DetachingItem(existing);
                newItem = resolved;
            }

            return true;
        }

        public bool Contains(T item)
        {
            return item != null && items.Exists(it => object.ReferenceEquals(it, item));
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

            return DetachingItem(item);
        }

        public T[] ToArray()
        {
            RequireIndexed();
            return indexes;
        }

        public U[] CreateCompatibleArray<U>(U freeSlot)
        {
            int last = LastIndex;
            var result = new U[last];
            while (last != 0)
            {
                result[--last] = freeSlot;
            }

            return result;
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

        private bool DetachingItem(T item)
        {
            int pos = items.FindIndex(it => object.ReferenceEquals(it, item));
            if (pos < 0)
            {
                return false;
            }

            item.Detaching(Scope);
            identityToItem.Remove(item.Identity);

            items.RemoveAt(pos);
            return true;
        }

        private void AttachedItem(T item)
        {
            identityToItem.Add(item.Identity, item);
            items.Add(item);
            item.Attached(Scope);
        }

        private static bool IsHiddenItem(T item)
        {
            return item != null && item.IsHidden;
        }

        private static bool IsPublicItem(T item)
        {
            return !item.IsHidden;
        }
    }
}