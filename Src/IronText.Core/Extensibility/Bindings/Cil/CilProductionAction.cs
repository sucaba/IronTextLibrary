using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using IronText.Framework.Reflection;

namespace IronText.Extensibility.Cil
{
    public class CilProductionAction : IProductionActionBinding
    {
        public readonly ProductionActionBuilder Builder;

        public CilProductionAction(ProductionActionBuilder builder)
        {
            this.Builder = builder;
        }

        internal object Hint { get; set; }
    }
}
