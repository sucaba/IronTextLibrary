using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Collections
{
    public class Joint : IEnumerable<object>
    {
        private readonly List all = new List();
        
        public bool Has<T>()
        {
            return all.Exists(typeof(T).IsInstanceOfType);
        }

        public T The<T>()
        {
            return (T)The(typeof(T));
        }

        private object The(Type type)
        {
            return all.Where(type.IsInstanceOfType).Single();
        }

        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        private object Get(Type type)
        {
            return all.Where(type.IsInstanceOfType).SingleOrDefault();
        }

        public IEnumerable<T> All<T>()
        {
            return all.OfType<T>();
        }

        public void Add(object value)
        {
            if (value != null)
            {
                all.Add(value);
            }
        }

        public void Add<T>(T value) where T : class
        {
            if (value != null)
            {
                all.Add(value);
            }
        }

        public void AddAll(Joint joint)
        {
            foreach (var inst in joint.all)
            {
                Add(inst);
            }
        }

        class List : List<object> { }

        public IEnumerator<object> GetEnumerator()
        {
            return all.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return all.GetEnumerator();
        }
    }
}
