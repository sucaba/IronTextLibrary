using System;

namespace IronText.Reflection.Managed
{
    public interface IContextResolverCode
    {
        void LdContext(string contextName);
    }
}
