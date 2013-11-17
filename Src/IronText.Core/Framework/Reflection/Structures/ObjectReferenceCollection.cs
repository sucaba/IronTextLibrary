using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public class ObjectReferenceCollection<T> 
        : Collection<T>
        , ITableObjectReferences<T>
        where T : ITableObject
    {
        private ITableObjectOwner<T> owner;

        public ObjectReferenceCollection()
        {
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
                    foreach (var item in this)
                    {
                        AddToOwner(item);
                    }
                }
                else if (value == null)
                {
                    this.owner = null;
                }
            }
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            AddToOwner(item);
        }

        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            AddToOwner(item);
        }

        private void AddToOwner(T item)
        {
            if (item.IsDetached)
            {
                owner.Add(item);
            }
        }
    }
}
