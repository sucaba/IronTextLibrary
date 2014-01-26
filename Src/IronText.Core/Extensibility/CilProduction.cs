using System;
using System.Linq;
using IronText.Framework;

namespace IronText.Extensibility
{
    public sealed class CilProduction : IEquatable<CilProduction>
    {
        public CilProduction(
            CilSymbolRef             left,
            CilSymbolRef[]           parts,
            CilProductionActionBuilder actionBuilder,
            Type                 instanceDeclaringType,
            bool                 isContextRule = false,
            Precedence           precedence = null)
        {
            this.Left                  = left;
            this.Parts                 = parts;
            this.ActionBuilder         = actionBuilder;
            this.InstanceDeclaringType = instanceDeclaringType;
            this.IsContextRule         = isContextRule;
            this.Precedence            = precedence;
        }

        public CilSymbolRef     Left { get; private set; }

        public CilSymbolRef[]   Parts { get; private set; }

        public Type         InstanceDeclaringType { get; private set; }

        public Precedence   Precedence { get; private set; }

        public bool         IsContextRule { get; private set; }

        internal int        Index  { get; set; }

        internal ICilMetadata Owner { get; set; }

        public CilProductionActionBuilder ActionBuilder { get; private set; }

        internal object Hint { get; set; }

        public override bool Equals(object obj)
        {
            var casted = obj as CilProduction;
            return Equals(casted);
        }

        public bool Equals(CilProduction other)
        {
            return other != null
                && object.Equals(Left, other.Left)
                && Enumerable.SequenceEqual(Parts, other.Parts)
                ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Left.GetHashCode() 
                    + Parts.Aggregate(0, (a, p) => a + p.GetHashCode());
            }
        }
    }
}
