using IronText.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantField : WantFieldAttr
    {
        [Produce("[", null, "]")]
        WantFieldAttr Repeat(int length);
    }
}
