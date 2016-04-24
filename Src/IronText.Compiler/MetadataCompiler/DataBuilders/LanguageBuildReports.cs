using IronText.Automata.Lalr1;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class LanguageBuildReports
    {
        public LanguageBuildReports(
            LanguageBuildConfig config,
            ILanguageSource     source,
            Grammar             grammar,
            IReportData         reportData)
        {
            if (!config.IsBootstrap)
            {
                foreach (var report in grammar.Reports)
                {
                    report.Build(reportData);
                }
            }
        }
    }
}
