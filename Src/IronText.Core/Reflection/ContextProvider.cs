using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ContextProvider : ReferenceCollection<ActionContext>
    {
        public ContextProvider()
        {
            this.Joint = new Joint();
        }

        public bool Provides(ActionContextRef reference)
        {
            return this.Any(c => c.Match(reference));
        }

        public Joint Joint { get; private set; }
    }
}
