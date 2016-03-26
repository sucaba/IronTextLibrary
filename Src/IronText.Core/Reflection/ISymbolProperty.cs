using IronText.Runtime.Semantics;

namespace IronText.Reflection
{
    public interface ISymbolProperty
    {
        Symbol Symbol { get; }

        string Name   { get; }

        IRuntimeValue ToRuntime(int offset);
    }
}
