using System;
using System.Collections.Generic;
using IronText.Extensibility;
using IronText.Framework.Reflection;

namespace IronText.Tests.TestUtils
{
    class StubTokenRefResolver : ICilSymbolResolver
    {
        public CilSymbol Resolve(CilSymbolRef tid)
        {
            throw new System.NotImplementedException();
        }

        public Symbol GetSymbol(CilSymbolRef tid)
        {
            return null;
        }

        public int GetId(CilSymbolRef tid)
        {
            return -1;
        }

        public void Link(CilSymbolRef tokenRefs)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CilSymbol> Definitions
        {
            get { throw new NotImplementedException(); }
        }

        public bool Contains(CilSymbolRef tokenRef)
        {
            throw new NotImplementedException();
        }
    }
}
