using System;

namespace IronText.Reflection.Managed
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
