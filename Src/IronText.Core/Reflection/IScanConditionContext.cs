using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public interface IScanConditionContext
    {
        IEbnfContext Parent { get; }

        ScanCondition Condition { get; }
    }
}
