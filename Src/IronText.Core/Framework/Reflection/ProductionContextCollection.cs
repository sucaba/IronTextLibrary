using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    public class ProductionContextCollection : IndexedCollection<ProductionContext,IEbnfContext>
    {
        public ProductionContextCollection(IEbnfContext ebnfGrammar)
            : base(ebnfGrammar)
        {
        }
    }
}
