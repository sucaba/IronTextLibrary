using IronText.Extensibility;

namespace IronText.MetadataCompiler
{
    interface IMetadataCollector
    {
        void AddMeta(ILanguageMetadata meta);
        void AddRule(ParseRule parseRule);
        void AddToken(TokenRef token);
    }
}
