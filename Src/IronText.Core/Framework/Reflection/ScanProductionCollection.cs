using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ScanProductionCollection : IndexedCollection<ScanProduction,IEbnfContext>
    {
        public ScanProductionCollection(EbnfGrammar ebnfGrammar)
            : base(ebnfGrammar)
        {
        }
    }
}
