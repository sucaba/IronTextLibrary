using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    public class ProductionContextProviderCollection : IndexedCollection<ProductionContextProvider,IEbnfContext>
    {
        public ProductionContextProviderCollection(IEbnfContext ebnfGrammar)
            : base(ebnfGrammar)
        {
        }
    }
}
