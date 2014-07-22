using IronText.Framework;
using IronText.Lib.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantFieldType
    {
        [Produce]
        WantFieldName OfType(Ref<Types> type);
    }
}
