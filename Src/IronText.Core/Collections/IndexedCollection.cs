using IronText.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Collections
{
    [Serializable]
    public class IndexedCollection<T, TScope> : IOwner<T>, IEnumerable<T>
        where T : class, IIndexable<TScope>, IHasIdentity
    {
        private const int InitialCapacity = 8;

        private int publicCount;
        private readonly List<T> items;

        [NonSerialized]
        private IDuplicateResolver<T> _duplicateResolver;
        private bool indexed   = false;
        private bool canModify = true;
        private T[] indexes;
        private readonly Dictionary<object,T> identityToItem = new Dictionary<object,T>();
        private readonly Dictionary<int, T> forcedIndexes = new Dictionary<int,T>();

        public IndexedCollection(TScope scope)
        {
            this.items = new List<T>(InitialCapacity);
            this.Scope = scope;
            this.DuplicateResolver = null;
        }

        public void BuildIndexes()
        {
            this.indexed   = true;
            this.canModify = false;

            int lastForcedIndex = forcedIndexes.Count == 0 ? 0 : forcedIndexes.Keys.Max() + 1;
            int lastIndex = Math.Max(items.Count, lastForcedIndex);
            this.indexes = new T[lastIndex];

            foreach (var pair in forcedIndexes)
            {
                indexes[pair.Key] = pair.Value;
                Impl(pair.Value).AssignIndex(pair.Key);
            }

            var itemsToIndex = items.Except(forcedIndexes.Values, ReferenceComparer<T>.Default).ToArray();
            var publicItems = itemsToIndex.Where(IsPublicItem);
            var privateItems = itemsToIndex.Where(item => !IsPublicItem(item));

            int count = indexes.Length;
            int i = 0;
            foreach (var item in publicItems)
            {
                while (indexes[i] != null)
                {
                    ++i;
                }

                indexes[i] = item;
                Impl(item).AssignIndex(i);
                ++i;
            }

            this.publicCount = i;
            
            foreach (var item in privateItems)
            {
                while (indexes[i] != null)
                {
                    ++i;
                }

                indexes[i] = item;
                Impl(item).AssignIndex(i);
                ++i;
            }

            int firstEmptyIndex = Array.IndexOf(indexes, null);
            if (firstEmptyIndex >= 0)
            {
                throw new InvalidOperationException("Index gap is not allowed. Index = " + firstEmptyIndex);
            }

            WhenDoneBuildIndexes();
        }

        protected virtual void WhenDoneBuildIndexes()
        {
        }

        public void RequireIndexed()
        {
            if (!indexed)
            {
                 throw new InvalidOperationException();
            }
        }

        public void RequireModifiable()
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
        public int     StartIndex  { get { return 0; } }

        /// <summary>
        /// Index of element following the last one
        /// </summary>
        public int     Count       { get { RequireIndexed(); return publicCount; } }

        public int     AllCount       { get { RequireIndexed(); return indexes.Length; } }

        public int     PrivateCount   { get { RequireIndexed(); return AllCount - Count; } }

        /// <summary>
        /// Hidden items
        /// </summary>
        public IEnumerable<T> Hidden
        {
            get {  return items.Where(item => item != null && item.IsSoftRemoved); }
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
                
                DetachingItem(existing, false);
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

        public void SoftRemove(T item)
        {
            RequireModifiable();

            DetachingItem(item, true);

            Impl(item).MarkSoftRemoved();
        }

        public bool Remove(T item)
        {
            RequireModifiable();

            if (item == null)
            {
                return false;
            }

            return DetachingItem(item, false);
        }

        public T[] ToArray()
        {
            RequireIndexed();
            return indexes;
        }

        public U[] CreateCompatibleArray<U>(Func<T, U> convert)
        {
            int last = Count;
            var result = new U[last];
            for (int i = 0; i != last; ++i)
            {
                result[i] = convert(indexes[i]);
            }
            
            return result;
        }

        public U[] CreateCompatibleArray<U>()
        {
            return new U[Count];
        }

        public U[] CreateCompatibleArray<U>(U freeSlot)
        {
            int last = Count;
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

        private bool DetachingItem(T item, bool softRemove)
        {
            int pos = items.FindIndex(it => object.ReferenceEquals(it, item));
            if (pos < 0)
            {
                return false;
            }

            Impl(item).Detaching(Scope);
            identityToItem.Remove(item.Identity);

            if (!softRemove)
            {
                items.RemoveAt(pos);
            }

            return true;
        }

        private void AttachedItem(T item)
        {
            identityToItem.Add(item.Identity, item);
            items.Add(item);
            Impl(item).Attached(Scope);
        }

        private static bool IsPublicItem(T item)
        {
            return !item.IsSoftRemoved;
        }

        private static IIndexableBackend<TScope> Impl(T item)
        {
            return (IIndexableBackend<TScope>)item;
        }
    }
}