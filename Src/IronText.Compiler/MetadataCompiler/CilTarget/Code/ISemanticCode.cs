using IronText.Reflection;

namespace IronText.MetadataCompiler
{
    interface ISemanticCode
    {
        bool LdSemantic(SemanticRef reference);
    }
}
