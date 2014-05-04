using IronText.Reflection;

namespace IronText.MetadataCompiler
{
    interface ISemanticLoader
    {
        bool LdSemantic(SemanticRef reference);
    }
}
