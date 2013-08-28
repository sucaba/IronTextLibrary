using System.Reflection;
using IronText.Framework;

namespace IronText.Build
{
    public class RequiredAssemblyProvider : AssemblyProviderBase
    {
        public RequiredAssemblyProvider(AssemblyName assemblyName) : base(assemblyName)
        {
        }

        protected override bool DoRebuild(ILogging logging, ref Assembly resource)
        {
            return true;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as RequiredAssemblyProvider;
            return casted != null
                && object.Equals(casted.AssemblyName, AssemblyName);
        }

        public override int GetHashCode()
        {
            return AssemblyName.GetHashCode();
        }
    }
}
