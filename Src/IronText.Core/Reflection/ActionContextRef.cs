using System;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ActionContextRef 
        : IndexableObject<ISharedGrammarEntities>
        , IEquatable<ActionContextRef>
    {
        public static readonly ActionContextRef None = new ActionContextRef("$none");

        public ActionContextRef(string name)
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
            var casted = obj as ActionContextRef;
            return Equals(casted);
        }

        public bool Equals(ActionContextRef other)
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
