using System;
using System.Linq;
using IronText.Framework;

namespace IronText.Extensibility
{
    public sealed class ParseRule : IEquatable<ParseRule>
    {
        public ParseRule(
            TokenRef             left,
            TokenRef[]           parts,
            ProductionActionBuilder actionBuilder,
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

        public TokenRef     Left { get; private set; }

        public TokenRef[]   Parts { get; private set; }

        public Type         InstanceDeclaringType { get; private set; }

        public Precedence   Precedence { get; private set; }

        public bool         IsContextRule { get; private set; }

        internal int        Index  { get; set; }

        internal ILanguageMetadata Owner { get; set; }

        public ProductionActionBuilder ActionBuilder { get; private set; }

        public override bool Equals(object obj)
        {
            var casted = obj as ParseRule;
            return Equals(casted);
        }

        public bool Equals(ParseRule other)
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
