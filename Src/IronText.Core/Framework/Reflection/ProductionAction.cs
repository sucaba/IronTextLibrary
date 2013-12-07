using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ProductionAction : IndexableObject<IEbnfContext>, ICloneable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">Zero-based index relative to a production start</param>
        /// <param name="argumentCount">Count of arguments consumed by the action</param>
        public ProductionAction(int start, int argumentCount)
        {
            this.Start         = start;
            this.ArgumentCount = argumentCount;
            this.Bindings      = new Collection<IProductionActionBinding>();
        }

        public int Start { get; private set; }

        public int ArgumentCount { get; private set; }

        public Collection<IProductionActionBinding> Bindings { get; private set; }

        public ProductionAction Clone()
        {
            var result = new ProductionAction(Start, ArgumentCount);
            foreach (var binding in Bindings)
            {
                result.Bindings.Add(binding);
            }

            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
