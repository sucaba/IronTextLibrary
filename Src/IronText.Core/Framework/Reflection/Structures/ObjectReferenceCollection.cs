using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public class ObjectReferenceCollection<T> : ITableObjectReferences<T>, IEnumerable<T>
        where T : class, ITableObject
    {
        private const int InitialCapacity = 1;

        private ITableObjectOwner<T> owner;
        private List<T>              items;

        public ObjectReferenceCollection()
        {
            items = new List<T>(1);
        }

        public int Count { get { return items.Count; } }

        public T First
        {
            get { return items[0]; }
        }

        public ITableObjectOwner<T> Owner
        {
            get { return this.owner; }
            set
            {
                if (value == owner)
                {
                }
                else if (owner == null)
                {
                    this.owner = value;

                    int count = Count;
                    var items = this.items;
                    for (int i = 0; i != count; ++i)
                    {
                        AddToOwner(items[i]);
                    }
                }
                else if (value == null)
                {
                    this.owner = null;
                }
            }
        }

        public void Add(T item)
        {
            AddToOwner(item);
            items.Add(item);
        }

        public bool Remove(T item)
        {
            return items.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        private void AddToOwner(T item)
        {
            if (owner != null && item.IsDetached)
            {
                owner.Add(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
