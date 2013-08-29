using System.Reflection;

namespace IronText.Build
{
    public class AssemblyDerivator : IDerivator
    {
        public AssemblyDerivator() { }

        public void Execute(ILogger logger, Assembly sourceAssembly, string derivedPath, params string[] resourceDirs)
        {
            var provider = new DerivedAssemblyProvider(sourceAssembly, derivedPath, resourceDirs);
            var resourceContext = ResourceContext.Instance;
            resourceContext.Logging = new LoggingAdapter(logger);
            resourceContext.LoadOrBuild(provider);
        }
    }
}
