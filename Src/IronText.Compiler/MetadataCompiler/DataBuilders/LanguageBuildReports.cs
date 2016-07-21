using IronText.DI;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class LanguageBuildReports : IHasSideEffects
    {
        public LanguageBuildReports(
            LanguageBuildConfig config,
            ILanguageSource     source,
            ReportCollection    reports,
            IReportData         reportData)
        {
            if (!config.IsBootstrap)
            {
                foreach (var report in reports)
                {
                    report.Build(reportData);
                }
            }
        }
    }
}
