using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    /// <summary>
    /// Simple unary action.
    /// </summary>
    public sealed class SimpleProductionAction : ProductionAction
    {
        public SimpleProductionAction(int argumentCount, ProductionContext context = null)
            : this(0, argumentCount, context)
        {
        }

        public SimpleProductionAction(int offset, int argumentCount, ProductionContext context = null)
        {
            this.Context       = context ?? ProductionContext.Global;
            this.Offset        = offset;
            this.ArgumentCount = argumentCount;
            this.Joint         = new Joint();
        }

        public Joint Joint { get; private set; }

        public int Offset { get; private set; }

        public int ArgumentCount { get; private set; }

        public ProductionContext Context { get; private set; }

        protected override ProductionAction DoClone()
        {
            var result = new SimpleProductionAction(Offset, ArgumentCount);
            result.Joint.AddAll(this.Joint);
            return result;
        }
    }
}
