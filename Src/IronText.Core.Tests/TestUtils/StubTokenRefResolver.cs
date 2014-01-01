using System;
using System.Collections.Generic;
using IronText.Extensibility;
using IronText.Framework.Reflection;

namespace IronText.Tests.TestUtils
{
    class StubTokenRefResolver : ITokenRefResolver
    {
        public TokenDef Resolve(TokenRef tid)
        {
            throw new System.NotImplementedException();
        }

        public Symbol GetSymbol(TokenRef tid)
        {
            return null;
        }

        public int GetId(TokenRef tid)
        {
            return -1;
        }

        public void Link(params TokenRef[] tokenRefs)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TokenDef> Definitions
        {
            get { throw new NotImplementedException(); }
        }

        public bool Contains(TokenRef tokenRef)
        {
            throw new NotImplementedException();
        }
    }
}
