using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    [Vocabulary]
    public interface IResolutionScopeNs
    {
        [Produce("[", null, "]")]
        Ref<ResolutionScopes> FromAssemblyName(Name1 name1);

        [Produce("[", ".module", null, "]")]
        Ref<ResolutionScopes> FromModuleName(Name1 name1);

        [Produce]
        Def<ResolutionScopes> DefineReferencedAssemblyName(Name1 name1);
    }
}
