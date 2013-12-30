using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    /// <summary>
    /// Composite action for invoking multiple underlying simple actions.
    /// The only use case of composite actions are inlined productions.
    /// </summary>
    public sealed class CompositeProductionAction : ProductionAction
    {
        public CompositeProductionAction()
        {
            this.Subactions = new Collection<SimpleProductionAction>();
        }

        public Collection<SimpleProductionAction> Subactions { get; private set; }

        protected override ProductionAction DoClone()
        {
            var result = new CompositeProductionAction();
            foreach (var action in Subactions)
            {
                result.Subactions.Add((SimpleProductionAction)action.Clone());
            }

            return result;
        }

        public int Offset { get { return 0; } }

        public int ArgumentCount
        {
            get
            {
                return 1 + Subactions.Sum(act => act.ArgumentCount - 1);
            }
        }
    }
}
