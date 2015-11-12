using IronText.Collections;
using System;

namespace IronText.Reflection
{
    [Serializable]
    internal class InheritedPropertyCollection : IndexedCollection<InheritedProperty, IGrammarScope>
    {
        public InheritedPropertyCollection(IGrammarScope scope)
            : base(scope)
        {
        }
    }
}
