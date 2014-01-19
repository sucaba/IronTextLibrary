using System;
using IronText.Extensibility;

namespace IronText.MetadataCompiler
{
    interface IScanDataCollector
    {
        void AddMeta(ICilMetadata meta);

        void AddScanRule(ICilScanRule rule);

        void AddScanMode(Type modeType);
    }
}
