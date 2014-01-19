using System.Collections.Generic;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    interface ITokenRefResolver
    {
        IEnumerable<CilSymbolDef> Definitions { get; }

        CilSymbolDef Resolve(CilSymbolRef tokenRef);

        Symbol GetSymbol(CilSymbolRef tokenRef);

        int GetId(CilSymbolRef tokenRef);

        void Link(params CilSymbolRef[] tokenRefs);

        bool Contains(CilSymbolRef tokenRef);
    }
}
