using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ProductionActionCollection : IndexedCollection<Production, IEbnfContext>
    {
        public ProductionActionCollection(IEbnfContext context)
            : base(context)
        {
        }
    }
}
