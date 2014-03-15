using System;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ForeignContext : IndexableObject<ISharedGrammarEntities>
    {
        public static readonly ForeignContext None = new ForeignContext("$none");

        public ForeignContext(string name)
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

        public bool Match(ForeignContextRef reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }

            return reference.UniqueName == this.UniqueName;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as ForeignContext;
            return Equals(casted);
        }

        public bool Equals(ForeignContext other)
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
