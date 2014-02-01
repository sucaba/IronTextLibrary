using System.Linq;
using IronText.Framework;
using IronText.Lib.Shared;
using IronText.Runtime;
using Mono.Cecil;

namespace IronText.Lib.IL.Backend.Cecil
{
    class CecilResolutionScopeNs : IResolutionScopeNs
    {
        private readonly ModuleDefinition module;

        public CecilResolutionScopeNs(ModuleDefinition module)
        {
            this.module = module;
        }

        public IParsing Parsing { get; set; }

        public Ref<ResolutionScopes> FromAssemblyName(Name1 name1)
        {
            var scope = GetScopeByName(name1, false);
            var def = new ValueSlot<ResolutionScopes> { Value = scope };
            return def.GetRef();
        }

        public Ref<ResolutionScopes> FromModuleName(Name1 name1)
        {
            var scope = GetScopeByName(name1, true);
            var def = new ValueSlot<ResolutionScopes> { Value = scope };
            return def.GetRef();
        }

        [Produce]
        public Def<ResolutionScopes> DefineReferencedAssemblyName(Name1 assemblyName)
        {
            IMetadataScope scope;

            if (assemblyName.FullName == "mscorlib")
            {
                // Ensure corlib was added 
                scope = this.module.TypeSystem.Corlib;
            }
            else
            {
                var assemblyRef = new AssemblyNameReference(assemblyName.FullName, null);
                module.AssemblyReferences.Add(assemblyRef);
                scope = assemblyRef;
            }

            return new ValueSlot<ResolutionScopes> { Value = scope };
        }

        private IMetadataScope GetScopeByName(Name1 name1, bool isModule)
        {
            if (isModule)
            {
                throw new SyntaxException(Parsing.Location, Parsing.HLocation, "Module scope resolution is not supported by now.");
            }

            string name = name1.FullName;
            AssemblyNameReference result;

            result = module.AssemblyReferences.FirstOrDefault(a => a.Name == name);
            if (result == null)
            {
                var msg = string.Format("Missing assembly reference '{0}'.", name);
                throw new SyntaxException(Parsing.Location, Parsing.HLocation, msg);
            }

            return result;
        }
    }
}
