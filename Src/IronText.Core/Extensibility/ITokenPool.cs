using System;

namespace IronText.Extensibility
{
    public interface ITokenPool
    {
        TokenRef AugmentedStart { get; }

        TokenRef ScanSkipToken { get; }

        TokenRef GetToken(Type tokenType);

        TokenRef GetLiteral(string literal);
    }
}
