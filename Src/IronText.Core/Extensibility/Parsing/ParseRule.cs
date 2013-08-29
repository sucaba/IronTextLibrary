using System;
using System.Linq;
using IronText.Framework;

namespace IronText.Extensibility
{
    public sealed class ParseRule
    {
        public TokenRef         Left;
        public TokenRef[]       Parts;
        public Type             InstanceDeclaringType;
        public GrammarActionBuilder ActionBuilder;
        public bool             IsContextRule;
        public Precedence       Precedence;
        internal ILanguageMetadata Owner;

        public int Index  { get; internal set; }

        public ParseRule()
        {
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
