using System;
using IronText.Collections;

namespace IronText.Reflection
{
    /// <summary>
    /// Simple unary action.
    /// </summary>
    public sealed class ProductionAction : ICloneable
    {
        public ProductionAction(int argumentCount, ProductionContext context = null)
            : this(0, argumentCount, context)
        {
        }

        public ProductionAction(int offset, int argumentCount, ProductionContext context = null)
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

        public ProductionAction Clone()
        {
            var result = new ProductionAction(Offset, ArgumentCount);
            result.Joint.AddAll(this.Joint);
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
