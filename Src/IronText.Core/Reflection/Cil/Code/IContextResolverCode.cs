using System;

namespace IronText.Reflection.Managed
{
    public interface IContextResolverCode
    {
        void LdContextOfType(Type type);
    }
}
