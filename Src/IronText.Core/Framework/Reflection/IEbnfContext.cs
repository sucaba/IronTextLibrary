using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public interface IEbnfContext
    {
        int StartToken { get; }

        SymbolCollection Symbols { get; }

        ProductionCollection Productions { get; }
    }
}
