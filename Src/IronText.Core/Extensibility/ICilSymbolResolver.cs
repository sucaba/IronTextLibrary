using System.Collections.Generic;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    interface ICilSymbolResolver
    {
        IEnumerable<CilSymbolDef> Definitions { get; }

        CilSymbolDef Resolve(CilSymbolRef symbol);

        Symbol GetSymbol(CilSymbolRef symbol);

        int    GetId(CilSymbolRef symbol);

        void   Link(CilSymbolRef symbol);

        bool   Contains(CilSymbolRef symbol);
    }
}
