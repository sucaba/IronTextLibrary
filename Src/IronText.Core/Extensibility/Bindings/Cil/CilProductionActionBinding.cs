using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using IronText.Framework.Reflection;

namespace IronText.Extensibility.Cil
{
    public class CilProductionActionBinding : IProductionActionBinding
    {
        public readonly ProductionActionBuilder Builder;

        public CilProductionActionBinding(ProductionActionBuilder builder)
        {
            this.Builder = builder;
        }
    }
}
