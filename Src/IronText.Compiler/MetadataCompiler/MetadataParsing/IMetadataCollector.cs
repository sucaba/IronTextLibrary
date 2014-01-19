using IronText.Extensibility;
using IronText.Extensibility.Cil;

namespace IronText.MetadataCompiler
{
    interface IMetadataCollector
    {
        void AddMeta(ILanguageMetadata meta);
        void AddRule(ILanguageMetadata meta, CilProductionDef parseRule);
        void AddToken(TokenRef token);
    }
}
