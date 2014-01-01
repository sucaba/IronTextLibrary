using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ScanCondition : IndexableObject<IEbnfContext>, IScanConditionContext
    {
        private readonly ScanProductionCollection   scanProductions;

        public ScanCondition()
        {
            this.scanProductions = new ScanProductionCollection(this);
            this.Bindings        = new Collection<IScanConditionBinding>();
        }

        public ScanProductionCollection ScanProducitons { get { return scanProductions; } }

        public Collection<IScanConditionBinding> Bindings { get; private set; }

        public ScanCondition Condition { get { return this; } }
    }
}
