using System;

namespace IronText.Reflection.Managed
{
    public class CilContext
    {
        public CilContext(Type contextType)
        {
            this.ContextType = contextType;
        }

        /// <summary>
        /// Context type
        /// </summary>
        public Type ContextType { get; private set; }
    }
}
