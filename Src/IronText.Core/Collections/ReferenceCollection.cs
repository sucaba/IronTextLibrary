using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework.Collections
{
    public class ReferenceCollection<T> : IReferenceContainer<T>, IEnumerable<T>
        where T : class
    {
        private const int InitialCapacity = 1;

        private IOwner<T> owner;
        private List<T>   items;

        public ReferenceCollection()
        {
            items = new List<T>(1);
        }

        public int Count { get { return items.Count; } }

        public T First()
        {
            if (items.Count == 0)
            {
                throw new InvalidOperationException("Cannot take first item of the empty collection.");
            }

            return items[0];
        }

        public IOwner<T> Owner
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
            if (owner != null)
            {
                owner.Acquire(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
