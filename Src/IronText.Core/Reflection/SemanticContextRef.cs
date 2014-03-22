using System;
using IronText.Collections;

namespace IronText.Reflection
{
    public class SemanticContextRef 
        : IndexableObject<ISharedGrammarEntities>
        , IEquatable<SemanticContextRef>
    {
        public static readonly SemanticContextRef None = new SemanticContextRef("$none");

        public SemanticContextRef(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.UniqueName = name;
            this.Joint      = new Joint();
        }

        public string UniqueName  { get; private set; }

        public Joint  Joint { get; private set; }

        public override bool Equals(object obj)
        {
            var casted = obj as SemanticContextRef;
            return Equals(casted);
        }

        public bool Equals(SemanticContextRef other)
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
