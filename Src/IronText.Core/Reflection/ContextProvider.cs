using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ContextProvider : ReferenceCollection<ProductionContext>
    {
        public ContextProvider()
        {
            this.Joint = new Joint();
        }

        public Joint Joint { get; private set; }
    }
}
