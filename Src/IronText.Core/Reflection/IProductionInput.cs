using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public interface IProductionInput
    {
        int  Size { get; }

        void CopyTo(Symbol[] output, int startIndex);
    }
}
