using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.Cecil
{
    public interface IScopeResolver
    {
        IMetadataScope Resolve(ModuleDefinition module, AssemblyNameReference assembly);
    }
}
