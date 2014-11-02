using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal interface ISymbolParser
    {
        Symbol ParseSymbol(string outcome);
    }
}
