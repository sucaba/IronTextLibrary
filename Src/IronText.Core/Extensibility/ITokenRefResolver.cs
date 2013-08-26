using System.Collections.Generic;

namespace IronText.Extensibility
{
    interface ITokenRefResolver
    {
        IEnumerable<TokenDef> Definitions { get; }

        TokenDef Resolve(TokenRef tokenRef);

        int GetId(TokenRef tokenRef);

        void Link(params TokenRef[] tokenRefs);
    }
}
