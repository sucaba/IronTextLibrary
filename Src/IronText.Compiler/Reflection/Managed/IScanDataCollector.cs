using System;
using IronText.Extensibility;

namespace IronText.Reflection.Managed
{
    interface IScanDataCollector
    {
        void AddMeta(ICilMetadata meta);

        void AddProduction(CilScanProduction rule);

        void AddCondition(Type conditionType);
    }
}
