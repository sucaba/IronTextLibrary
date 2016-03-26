using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Runtime.Semantics
{
    public interface IRuntimeValue
    {
        object Eval(IStackLookback<ActionNode> lookback);
    }
}
