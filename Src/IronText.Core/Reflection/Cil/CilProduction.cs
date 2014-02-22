using System;
using System.Linq;

namespace IronText.Reflection.Managed
{
    public sealed class CilProduction : IEquatable<CilProduction>
    {
        public CilProduction(CilSymbolRef outcome, CilSymbolRef[] pattern, CilContext context, CilProductionActionBuilder actionBuilder, Type contextType, Precedence precedence = null)
        {
            this.Outcome             = outcome;
            this.Pattern             = pattern;
            this.Context             = context;
            this.ActionBuilder       = actionBuilder;
            this.ContextType         = contextType;
            this.Precedence          = precedence;
        }

        public CilSymbolRef   Outcome       { get; private set; }

        public CilSymbolRef[] Pattern       { get; private set; }

        public Type           ContextType   { get; private set; }

        public Precedence     Precedence    { get; private set; }

        public CilContext     Context       { get; private set; }

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
