using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ScanCondition : IndexableObject<IEbnfContext>
    {
        public ScanCondition(string name)
        {
            this.Name            = name;
            this.ScanProductions = new ReferenceCollection<ScanProduction>();
            this.Bindings        = new Collection<IScanConditionBinding>();
        }

        public string Name { get; private set; }

        public ReferenceCollection<ScanProduction> ScanProductions { get; private set; }

        public Collection<IScanConditionBinding> Bindings { get; private set; }

        public ScanCondition Condition { get { return this; } }

        protected override void DoAttached()
        {
            base.DoAttached();
            ScanProductions.Owner = Context.ScanProductions;
        }

        protected override void DoDetaching()
        {
            ScanProductions.Owner = null;
            base.DoDetaching();
        }
    }
}
