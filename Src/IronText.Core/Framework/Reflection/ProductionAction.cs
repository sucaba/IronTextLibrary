using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Action applicatin invariant: 1 == inputSize + sum(1 - actionLen)
    /// Refactor to hierarhy:
    /// - ProductionActionBase
    /// - SimpleProductionAction - single reduce aciton
    /// - CompositeProductionAction - multi-reduce action. has multiple simple actions
    /// </remarks>
    public class ProductionAction : IndexableObject<IEbnfContext>, ICloneable
    {
        public ProductionAction(int backOffset)
            : this(backOffset, backOffset)
        {
        }

        /// <param name="backOffset">Zero-based index relative to a production end</param>
        /// <param name="argumentCount">Count of arguments consumed by the action</param>
        public ProductionAction(int backOffset, int argumentCount)
        {
            this.BackOffset    = backOffset;
            this.ArgumentCount = argumentCount;
            this.Bindings      = new Collection<IProductionActionBinding>();
        }

        public int BackOffset { get; private set; }

        public int ArgumentCount { get; private set; }

        public Collection<IProductionActionBinding> Bindings { get; private set; }

        public ProductionAction Clone()
        {
            var result = new ProductionAction(BackOffset, ArgumentCount);
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
