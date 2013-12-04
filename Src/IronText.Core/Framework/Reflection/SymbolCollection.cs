using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class SymbolCollection : IndexedCollection<SymbolBase, IEbnfContext>
    {
        public SymbolCollection(IEbnfContext context)
            : base(context)
        {
        }

        public Symbol Add(string name, SymbolCategory categories = SymbolCategory.None)
        {
            var result = new Symbol(name) { Categories = categories };
            Add(result);
            return result;
        }
    }
}
