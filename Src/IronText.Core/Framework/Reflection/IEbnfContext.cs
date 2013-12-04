using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public interface IEbnfContext
    {
        Symbol Start { get; }

        SymbolCollection Symbols { get; }

        ProductionCollection Productions { get; }
    }
}
