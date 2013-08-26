using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    [Vocabulary]
    public interface IResolutionScopeNs
    {
        [Parse("[", null, "]")]
        Ref<ResolutionScopes> FromAssemblyName(Name1 name1);

        [Parse("[", ".module", null, "]")]
        Ref<ResolutionScopes> FromModuleName(Name1 name1);

        [Parse]
        Def<ResolutionScopes> DefineReferencedAssemblyName(Name1 name1);
    }
}
