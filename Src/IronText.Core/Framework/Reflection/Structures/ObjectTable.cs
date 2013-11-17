using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public class ObjectTable<T> : Collection<T>
        where T : ITableObject
    {
        public T Get(int id)
        {
            int index = TableIndexing.IndexFromId(id);
            return this[index];
        }

        public bool TryGet(int id, out T output)
        {
            int index = TableIndexing.IndexFromId(id);
            if (index < 0 || index >= Count)
            {
                output = default(T);
                return false;
            }

            output = this[index];
            return true;
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.Detach();
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            item.Attach(TableIndexing.IdFromIndex(index));
        }

        protected override void RemoveItem(int index)
        {
            this[index].Detach();
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            this[index].Detach();
            base.SetItem(index, item);
            item.Attach(TableIndexing.IdFromIndex(index));
        }
    }
}