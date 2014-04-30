using System;
using IronText.Collections;

namespace IronText.Reflection
{
    public class SemanticValue : IndexableObject<ISharedGrammarEntities>
    {
        public static readonly SemanticValue None = new SemanticValue("$none");

        public SemanticValue(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.UniqueName  = name;
            this.Joint = new Joint();
        }

        public string UniqueName  { get; private set; }

        public Joint  Joint { get; private set; }

        public bool Match(SemanticRef reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }

            return reference.UniqueName == this.UniqueName;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as SemanticValue;
            return Equals(casted);
        }

        public bool Equals(SemanticValue other)
        {
            return other != null && UniqueName == other.UniqueName;
        }

        public override int GetHashCode()
        {
            return UniqueName.GetHashCode();
        }

        public override string ToString() { return UniqueName; }
    }
}
