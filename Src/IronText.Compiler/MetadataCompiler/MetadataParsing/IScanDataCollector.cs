using System;
using IronText.Extensibility;

namespace IronText.MetadataCompiler
{
    interface IScanDataCollector
    {
        void AddMeta(ILanguageMetadata meta);

        void AddScanRule(ScanRule rule);

        void AddScanMode(Type modeType);
    }
}
