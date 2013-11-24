using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Collections
{
    public class IndexedObject : IIndexable
    {
        public const int NoId = -1;

        public IndexedObject()
        {
            Index = NoId;
        }

        public int Index { get; private set; }

        public bool IsDetached { get { return NoId == Index; } }

        void IIndexable.Attach(int id)
        {
            if (!IsDetached)
            {
                throw new InvalidOperationException("Object is already attached to a table.");
            }

            Index = id;
        }

        void IIndexable.Detach()
        {
            Index = NoId;
        }
    }
}
