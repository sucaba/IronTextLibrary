using System;
using IronText.Collections;

namespace IronText.Reflection
{
    /// <summary>
    /// Simple unary action.
    /// </summary>
    public sealed class ForeignAction : ICloneable
    {
        public ForeignAction(int argumentCount, ForeignContextRef context = null)
            : this(0, argumentCount, context)
        {
        }

        public ForeignAction(int offset, int argumentCount, ForeignContextRef context = null)
        {
            this.ContextRef    = context ?? ForeignContextRef.None;
            this.Offset        = offset;
            this.ArgumentCount = argumentCount;
            this.Joint         = new Joint();
        }

        public Joint Joint { get; private set; }

        public int Offset { get; private set; }

        public int ArgumentCount { get; private set; }

        public ForeignContextRef ContextRef { get; private set; }

        public ForeignAction Clone()
        {
            var result = new ForeignAction(Offset, ArgumentCount);
            result.Joint.AddAll(this.Joint);
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
