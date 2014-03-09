using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Reflection.Managed
{
    public sealed class CilProduction : IEquatable<CilProduction>
    {
        public CilProduction(CilSymbolRef outcome, IEnumerable<CilSymbolRef> pattern, CilContextRef context, CilProductionActionBuilder actionBuilder, Precedence precedence = null)
        {
            this.Outcome             = outcome;
            this.Pattern             = pattern.ToArray();
            this.Context             = context;
            this.ActionBuilder       = actionBuilder;
            this.Precedence          = precedence;
        }

        public CilContextRef  Context       { get; private set; }

        public CilSymbolRef   Outcome       { get; private set; }

        public CilSymbolRef[] Pattern       { get; private set; }

        public Precedence     Precedence    { get; private set; }

        internal object       Owner         { get; set; }

        public CilProductionActionBuilder ActionBuilder         { get; private set; }

        public override bool Equals(object obj)
        {
            var casted = obj as CilProduction;
            return Equals(casted);
        }

        public bool Equals(CilProduction other)
        {
            return other != null
                && object.Equals(Outcome, other.Outcome)
                && Enumerable.SequenceEqual(Pattern, other.Pattern)
                ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Outcome.GetHashCode() 
                    + Pattern.Aggregate(0, (a, p) => a + p.GetHashCode());
            }
        }
    }
}
