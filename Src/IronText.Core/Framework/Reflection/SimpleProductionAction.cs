using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    /// <summary>
    /// Simple unary action.
    /// </summary>
    public sealed class SimpleProductionAction : ProductionAction
    {
        public SimpleProductionAction(int argumentCount)
            : this(0, argumentCount)
        {
        }

        public SimpleProductionAction(int offset, int argumentCount)
        {
            this.Offset        = offset;
            this.ArgumentCount = argumentCount;
            this.Bindings      = new Collection<IProductionActionBinding>();
        }

        public Collection<IProductionActionBinding> Bindings { get; private set; }

        public int Offset { get; private set; }

        public int ArgumentCount { get; private set; }

        protected override ProductionAction DoClone()
        {
            var result = new SimpleProductionAction(Offset, ArgumentCount);
            foreach (var binding in Bindings)
            {
                result.Bindings.Add(binding);
            }

            return result;
        }
    }
}
