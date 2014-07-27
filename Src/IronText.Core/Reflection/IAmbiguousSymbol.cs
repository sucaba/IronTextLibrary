using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public interface IAmbiguousSymbol
    {
        string Name { get; }

        int Index { get; }
    }
}
