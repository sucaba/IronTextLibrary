using System;
using IronText.Extensibility;

namespace IronText.Reflection.Managed
{
    interface IScanDataCollector
    {
        void AddMeta(ICilMetadata meta);

        void AddScanRule(CilScanProduction rule);

        void AddScanMode(Type modeType);
    }
}
