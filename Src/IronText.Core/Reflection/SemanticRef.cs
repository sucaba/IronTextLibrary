using System;
using IronText.Collections;

namespace IronText.Reflection
{
    public class SemanticRef : IEquatable<SemanticRef>
    {
        public static readonly SemanticRef None = new SemanticRef("$none");

        public SemanticRef(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.UniqueName = name;
        }

        public string UniqueName  { get; private set; }

        public override bool Equals(object obj)
        {
            var casted = obj as SemanticRef;
            return Equals(casted);
        }

        public bool Equals(SemanticRef other)
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
