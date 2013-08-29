using IronText.Extensibility;

namespace IronText.MetadataCompiler
{
    interface IMetadataCollector
    {
        void AddMeta(ILanguageMetadata meta);
        void AddRule(ILanguageMetadata meta, ParseRule parseRule);
        void AddToken(TokenRef token);
    }
}
