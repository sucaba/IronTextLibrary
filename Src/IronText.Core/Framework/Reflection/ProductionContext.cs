using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    public class ProductionContext : IndexableObject<IEbnfContext>
    {
        public ProductionContext(string name)
        {
            this.Name  = name;
            this.Joint = new Joint();
        }

        public string Name  { get; private set; }

        public Joint  Joint { get; private set; }
    }
}
