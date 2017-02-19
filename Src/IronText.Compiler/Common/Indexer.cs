using System.Collections.Generic;

namespace IronText.Common
{
    class Indexer<T>
    {
        public int Get(T item) => GetOrAdd(item);
        public int this[T item] => GetOrAdd(item);

        private readonly Dictionary<T,int> itemToIndex = new Dictionary<T,int>();
        private readonly List<T> items = new List<T>();

        public void AddRange(IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
            {
                GetOrAdd(item);
            }
        }

        private int GetOrAdd(T item)
        {
            int result;
            if (!itemToIndex.TryGetValue(item, out result))
            {
                result = items.Count;
                itemToIndex.Add(item, result);
                items.Add(item);
            }

            return result;
        }
    }
}
