using IronText.Extensibility;
using IronText.Reflection.Managed;

namespace IronText.Reflection.Managed
{
    interface IMetadataCollector
    {
        void AddMeta(ICilMetadata meta);
        void AddRule(ICilMetadata meta, CilProduction parseRule);
        void AddToken(CilSymbolRef token);
    }
}
