using System;
using IronText.Collections;

namespace IronText.Reflection
{
    [Serializable]
    public class SemanticRef : IEquatable<SemanticRef>
    {
        public static readonly SemanticRef None = new SemanticRef(SharedConstants.NullSemanticRefName);

        public static SemanticRef ByName(string name)
        {
            if (name == SharedConstants.NullSemanticRefName)
            {
                return None;
            }

            return new SemanticRef(name);
        }

        private SemanticRef(string uniqueName)
        {
            this.UniqueName = uniqueName;
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

        public static bool operator ==(SemanticRef x, SemanticRef y)
        {
            return ((object)x == (object)y) || (null != (object)x && null != (object)y && x.UniqueName == y.UniqueName);
        }

        public static bool operator !=(SemanticRef x, SemanticRef y) { return !(x == y); }

        public override string ToString() { return UniqueName; }
    }
}
