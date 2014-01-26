using System.Collections.Generic;
using IronText.Reflection;

namespace IronText.Extensibility
{
    interface ICilSymbolResolver
    {
        IEnumerable<CilSymbol> Definitions { get; }

        CilSymbol Resolve(CilSymbolRef symbol);

        Symbol GetSymbol(CilSymbolRef symbol);

        int    GetId(CilSymbolRef symbol);

        void   Link(CilSymbolRef symbol);

        bool   Contains(CilSymbolRef symbol);
    }
}
