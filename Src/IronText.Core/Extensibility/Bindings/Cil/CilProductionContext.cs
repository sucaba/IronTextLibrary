using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility.Cil
{
    public class CilProductionContext
    {
        public CilProductionContext(Type contextType)
        {
            this.ContextType = contextType;
        }

        /// <summary>
        /// Context type
        /// </summary>
        public Type ContextType { get; private set; }
    }
}
