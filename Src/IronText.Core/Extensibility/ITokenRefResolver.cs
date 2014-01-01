using System.Collections.Generic;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    interface ITokenRefResolver
    {
        IEnumerable<TokenDef> Definitions { get; }

        TokenDef Resolve(TokenRef tokenRef);

        Symbol GetSymbol(TokenRef tokenRef);

        int GetId(TokenRef tokenRef);

        void Link(params TokenRef[] tokenRefs);

        bool Contains(TokenRef tokenRef);
    }
}
