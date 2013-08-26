using System.Reflection;

namespace IronText.Build
{
    public interface IMetadataSyntax<TContext>
    {
        IDerivedBuilder<TContext> GetBuilder(Assembly assembly);
    }
}
