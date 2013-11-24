using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ProductionAction : IndexedObject
    {
        public ProductionAction()
        {
            this.Bindings = new Collection<IProductionActionBinding>();
        }

        public Collection<IProductionActionBinding> Bindings { get; private set; }
    }
}
