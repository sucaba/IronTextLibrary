using Mono.Cecil;

namespace IronText.Lib.IL.Backend.Cecil
{
    class RewriteScopeResolver : IScopeResolver
    {
        public IMetadataScope Resolve(ModuleDefinition module, AssemblyNameReference assembly)
        {
            if (assembly.Name == module.Assembly.Name.Name)
            {
                return module;
            }

            return assembly;
        }
    }
}
