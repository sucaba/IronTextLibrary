using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ProductionActionCollection : IndexedCollection<Production, IEbnfEntities>
    {
        public ProductionActionCollection(IEbnfEntities context)
            : base(context)
        {
        }
    }
}
