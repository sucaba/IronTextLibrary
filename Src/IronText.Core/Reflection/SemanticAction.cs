using System;
using IronText.Collections;

namespace IronText.Reflection
{
    /// <summary>
    /// Simple unary action.
    /// </summary>
    public sealed class SemanticAction : ICloneable
    {
        public SemanticAction(int argumentCount, SemanticContextRef context = null)
            : this(0, argumentCount, context)
        {
        }

        public SemanticAction(int offset, int argumentCount, SemanticContextRef context = null)
        {
            this.ContextRef    = context ?? SemanticContextRef.None;
            this.Offset        = offset;
            this.ArgumentCount = argumentCount;
            this.Joint         = new Joint();
        }

        public Joint Joint { get; private set; }

        public int Offset { get; private set; }

        public int ArgumentCount { get; private set; }

        public SemanticContextRef ContextRef { get; private set; }

        public SemanticAction Clone()
        {
            var result = new SemanticAction(Offset, ArgumentCount);
            result.Joint.AddAll(this.Joint);
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
