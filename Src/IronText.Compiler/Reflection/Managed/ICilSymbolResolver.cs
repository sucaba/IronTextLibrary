using System.Collections.Generic;
using IronText.Reflection;

namespace IronText.Reflection.Managed
{
    interface ICilSymbolResolver
    {
        IEnumerable<CilSymbol> Definitions { get; }

        CilSymbol Resolve(CilSymbolRef symbol);

        Symbol    GetSymbol(CilSymbolRef symbol);

        void      Link(CilSymbolRef symbol);

        bool      Contains(CilSymbolRef symbol);
    }
}
