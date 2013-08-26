using System.Reflection;

namespace IronText.Build
{
    public class AssemblyDerivator : IDerivator
    {
        public AssemblyDerivator() { }

        public void Execute(ILogger logger, Assembly sourceAssembly, string derivedPath)
        {
            var provider = new DerivedAssemblyProvider(sourceAssembly, derivedPath);
            var resourceContext = ResourceContext.Instance;
            resourceContext.Logging = new LoggingAdapter(logger);
            resourceContext.LoadOrBuild(provider);
        }
    }
}
