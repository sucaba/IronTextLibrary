using System;

namespace IronText.Extensibility
{
    public interface ITokenPool
    {
        CilSymbolRef AugmentedStart { get; }

        CilSymbolRef ScanSkipToken { get; }

        CilSymbolRef GetToken(Type tokenType);

        CilSymbolRef GetLiteral(string literal);
    }
}
