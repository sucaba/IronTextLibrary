using IronText.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantFieldAttr 
        : WantFieldAttrThen<WantFieldAttr>
        , WantFieldType
    {
    }
}
