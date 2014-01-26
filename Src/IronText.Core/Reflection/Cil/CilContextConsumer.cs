using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection.Managed
{
    public class CilContextConsumer
    {
        public CilContextConsumer(Type contextType)
        {
            this.ContextType = contextType;
        }

        /// <summary>
        /// Context type
        /// </summary>
        public Type ContextType { get; private set; }
    }
}
