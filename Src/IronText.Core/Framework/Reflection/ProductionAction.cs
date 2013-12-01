using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ProductionAction : IndexableObject<IEbnfContext>, ICloneable
    {
        public ProductionAction()
        {
            this.Bindings = new Collection<IProductionActionBinding>();
        }

        public Collection<IProductionActionBinding> Bindings { get; private set; }

        public ProductionAction Clone()
        {
            var result = new ProductionAction();
            foreach (var binding in Bindings)
            {
                result.Bindings.Add(binding);
            }

            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
