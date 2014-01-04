using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    public class Merger : IndexableObject<IEbnfContext>
    {
        private readonly Symbol symbol;

        public Merger(Symbol symbol)
        {
            this.symbol = symbol;
            this.Joint = new Joint();
        }

        public Joint Joint { get; private set; }
    }
}
