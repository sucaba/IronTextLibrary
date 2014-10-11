using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
