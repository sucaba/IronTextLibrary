using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    public sealed class ProductionContextProvider : IndexableObject<IEbnfContext>
    {
        public ProductionContextProvider()
        {
            this.AvailableContexts = new ReferenceCollection<ProductionContext>();
            this.Joint = new Joint();
        }

        protected override void DoAttached()
        {
            base.DoAttached();
            AvailableContexts.Owner = Context.ProductionContexts;
        }

        protected override void DoDetaching()
        {
            AvailableContexts.Owner = null;
            base.DoDetaching();
        }

        public ReferenceCollection<ProductionContext> AvailableContexts { get; private set; }

        public Joint Joint { get; private set; }
    }
}
