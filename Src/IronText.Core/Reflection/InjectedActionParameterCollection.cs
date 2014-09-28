using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public class InjectedActionParameterCollection : IndexedCollection<InjectedActionParameter, IGrammarScope>
    {
        public InjectedActionParameterCollection(IGrammarScope scope)
            : base(scope)
        {
        }
    }
}
