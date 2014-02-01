using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ProductionContextCollection : IndexedCollection<ProductionContext,IEbnfEntities>
    {
        public ProductionContextCollection(IEbnfEntities ebnfGrammar)
            : base(ebnfGrammar)
        {
        }

        public bool FindOrAdd(string name, out ProductionContext output)
        {
            foreach (var item in this)
            {
                if (item.Name == name)
                {
                    output = item;
                    return false;
                }
            }

            output = Add(new ProductionContext(name));
            return true;
        }
    }
}
