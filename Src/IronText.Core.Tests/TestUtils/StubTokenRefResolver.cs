using System;
using System.Collections.Generic;
using IronText.Extensibility;

namespace IronText.Tests.TestUtils
{
    class StubTokenRefResolver : ITokenRefResolver
    {
        public TokenDef Resolve(TokenRef tid)
        {
            throw new System.NotImplementedException();
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
    }
}
