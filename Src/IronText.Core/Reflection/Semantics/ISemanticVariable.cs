using IronText.Runtime.Semantics;

namespace IronText.Reflection
{
    public interface ISemanticVariable
    {
        IRuntimeVariable ToRuntime(int currentProductionPosition);
    }
}