using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public class ProductionAction : TableObject
    {
        public ProductionAction()
        {
            this.Bindings = new Collection<IProductionActionBinding>();
        }

        public Collection<IProductionActionBinding> Bindings { get; private set; }
    }
}
