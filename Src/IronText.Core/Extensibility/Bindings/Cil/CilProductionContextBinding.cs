﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility.Bindings.Cil
{
    public class CilProductionContextBinding
    {
        public CilProductionContextBinding(Type contextType)
        {
            this.ContextType = contextType;
        }

        /// <summary>
        /// Context type
        /// </summary>
        public Type ContextType { get; private set; }
    }
}
