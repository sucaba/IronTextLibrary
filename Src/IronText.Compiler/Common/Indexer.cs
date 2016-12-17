using System.Collections.Generic;

namespace IronText.Common
{
    class Indexer<T>
    {
        private readonly Dictionary<T,int> itemToIndex = new Dictionary<T,int>();
        private readonly List<T> items = new List<T>();

        public void AddRange(IEnumerable<T> newItems)
        {
            this.items.AddRange(newItems);
            foreach (var item in newItems)
            {
                if (!itemToIndex.ContainsKey(item))
                {
                    itemToIndex.Add(item, items.Count);
                    items.Add(item);
                }
            }
        }

        public int Get(T item) => itemToIndex[item];
        
        public int this[T item] => Get(item);
    }
}
