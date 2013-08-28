using System;
using IronText.Extensibility;

namespace IronText.MetadataCompiler
{
    interface IScanDataCollector
    {
        void AddMeta(ILanguageMetadata meta);

        void AddScanRule(IScanRule rule);

        void AddScanMode(Type modeType);
    }
}
