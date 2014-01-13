using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility.Bindings.Cil
{
    public class CilProductionContextProviderBinding
    {
        public CilProductionContextProviderBinding(Type tokenType)
        {
            this.TokenType = tokenType;
        }

        /// <summary>
        /// Context token type <seealso cref="DemandAttribute"/>
        /// </summary>
        public Type TokenType { get; private set; }
    }
}
