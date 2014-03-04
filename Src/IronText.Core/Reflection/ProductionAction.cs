using System;
using IronText.Collections;

namespace IronText.Reflection
{
    /// <summary>
    /// Simple unary action.
    /// </summary>
    public sealed class ProductionAction : ICloneable
    {
        public ProductionAction(int argumentCount, ActionContextRef context = null)
            : this(0, argumentCount, context)
        {
        }

        public ProductionAction(int offset, int argumentCount, ActionContextRef context = null)
        {
            this.ContextRef    = context ?? ActionContextRef.None;
            this.Offset        = offset;
            this.ArgumentCount = argumentCount;
            this.Joint         = new Joint();
        }

        public Joint Joint { get; private set; }

        public int Offset { get; private set; }

        public int ArgumentCount { get; private set; }

        public ActionContextRef ContextRef { get; private set; }

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
