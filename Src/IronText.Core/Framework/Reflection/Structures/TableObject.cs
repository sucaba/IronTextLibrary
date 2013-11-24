using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public class TableObject : ITableObject
    {
        public TableObject()
        {
            Index = TableIndexing.NoId;
        }

        public int Index { get; private set; }

        public bool IsDetached { get { return TableIndexing.NoId == Index; } }

        void ITableObject.Attach(int id)
        {
            if (!IsDetached)
            {
                throw new InvalidOperationException("Object is already attached to a table.");
            }

            Index = id;
        }

        void ITableObject.Detach()
        {
            Index = -1;
        }
    }
}
