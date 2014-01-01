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
        public ScanCondition()
        {
            this.Bindings = new Collection<IScanConditionBinding>();
        }

        public Collection<IScanConditionBinding> Bindings { get; private set; }
    }
}
