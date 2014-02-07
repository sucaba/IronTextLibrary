using IronText.Extensibility;

namespace IronText.Reflection.Managed
{
    interface IMetadataCollector
    {
        void AddMeta(ICilMetadata meta);

        void AddProduction(ICilMetadata meta, CilProduction production);

        void AddSymbol(CilSymbolRef symbol);
    }
}
