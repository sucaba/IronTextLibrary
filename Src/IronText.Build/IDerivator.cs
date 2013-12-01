using System.Reflection;

namespace IronText.Build
{
    public interface IDerivator
    {
        void Execute(
            ILogger         logger,
            Assembly        sourceAssembly,
            string          derivedPath,
            params string[] resourceDirs);
    }
}
