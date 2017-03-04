using IronText.DI;
using IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning;
using IronText.Reporting;
using IronText.Runtime;
using System;

namespace IronText.MetadataCompiler.DataBuilders.Reporting
{
    class ReportDataSelector : IDynamicDependency<IReportData>
    {
        public ReportDataSelector(ParserRuntime runtime)
        {
            Implementation = runtime == ParserRuntime.Generic
                ? typeof(TurnBasedReportData)
                : typeof(CanonicalReportData);
        }

        public Type Implementation { get; }
    }
}
