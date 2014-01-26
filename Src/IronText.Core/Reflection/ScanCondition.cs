using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ScanCondition : IndexableObject<IEbnfContext>
    {
        public ScanCondition(string name)
        {
            this.Name            = name;
            this.ScanProductions = new ReferenceCollection<ScanProduction>();
            this.Joint           = new Joint();
        }

        public string Name { get; private set; }

        public ReferenceCollection<ScanProduction> ScanProductions { get; private set; }

        public Joint Joint { get; private set; }

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
