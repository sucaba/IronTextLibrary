using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    public class ContextProvider
    {
        public ContextProvider(Type contextType)
        {
            this.ContextType = contextType;
        }

        public Type ContextType { get; private set; }
    }
}
