using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Collections
{
    public class TypeMap<T> : IEnumerable<KeyValuePair<Type,T>>
        where T : class
    {
        private readonly Dictionary<Type, T> map;

        public TypeMap()
        {
            this.map = new Dictionary<Type,T>();
        }

        public T this[Type key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        public T Get<Key>() { return Get(typeof(Key)); }

        public void Set<Key>(T value) { Set(typeof(Key), value); }

        public T Get(Type key)
        {
            T result;
            if (!map.TryGetValue(key, out result))
            {
                result = null;
            }

            return result;
        }

        public void Set(Type key, T value)
        {
            map[key] = value;
        }

        public void Add(Type key, T value)
        {
            map.Add(key, value);
        }

        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator()
        {
            return map.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return map.GetEnumerator();
        }
    }
}
