using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Reflection;

namespace IronText.Analysis
{
    interface IEbnfConverter
    {
        SymbolBase Convert(SymbolBase source);

        Symbol Convert(Symbol source);

        AmbiguousSymbol Convert(AmbiguousSymbol source);

        Production Convert(Production source);
    }
}
