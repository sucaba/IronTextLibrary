using System;

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
