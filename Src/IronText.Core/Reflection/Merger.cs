using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class Merger : IndexableObject<IEbnfContext>
    {
        public Merger(Symbol symbol)
        {
            this.Symbol = symbol;
            this.Joint = new Joint();
        }

        public Symbol Symbol { get; private set; }

        public Joint Joint { get; private set; }
    }
}
