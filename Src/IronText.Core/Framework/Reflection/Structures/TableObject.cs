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
            Id = TableIndexing.NoId;
        }

        public int Id { get; private set; }

        public bool IsDetached { get { return TableIndexing.NoId == Id; } }

        void ITableObject.Attach(int id)
        {
            if (!IsDetached)
            {
                throw new InvalidOperationException("Object is already attached to a table.");
            }

            Id = id;
        }

        void ITableObject.Detach()
        {
            Id = -1;
        }
    }
}
