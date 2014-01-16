using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    public class CilContextProvider
    {
        public CilContextProvider(Type providerType)
        {
            this.ProviderType = providerType;
        }

        public Type ProviderType { get; private set; }
    }
}
