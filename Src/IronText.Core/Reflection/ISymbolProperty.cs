using IronText.Runtime.Semantics;

namespace IronText.Reflection
{
    public interface ISymbolProperty
    {
        Symbol Symbol { get; }

        string Name   { get; }

        IRuntimeValue    ToRuntimeValue(int offset);

        IRuntimeVariable ToRuntimeVariable(int offset);
    }
}
