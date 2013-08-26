using System;
using System.Linq;
using IronText.Framework;

namespace IronText.Extensibility
{
    public sealed class ParseRule
    {
        private readonly object identity;

        public TokenRef         Left;
        public TokenRef[]       Parts;
        public Type             InstanceDeclaringType;
        public bool             IsContextRule;
        public Precedence       Precedence;
        public GrammarActionBuilder ActionBuilder;

        public int Index  { get; internal set; }

        public ParseRule(object identity)
        {
            this.identity = identity;
        }

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
                && object.Equals(identity, other.identity)
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
