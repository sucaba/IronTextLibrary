using System.Reflection;

namespace IronText.Build
{
    public interface IDerivedAssemblyBuildScenario
    {
        IExternalResourceProvider<Assembly> GetAssemblyProvider(Assembly sourceAssembly);
    }
}
