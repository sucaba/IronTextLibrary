using IronText.Automata.Lalr1;
using IronText.Build;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;
using IronText.DI;
using IronText.Automata.Regular;

namespace IronText.MetadataCompiler
{
    internal class LanguageDataProvider : ResourceGetter<LanguageData>
    {
        private readonly ILanguageSource     source;
        private readonly LanguageBuildConfig config;

        public LanguageDataProvider(ILanguageSource source, bool bootstrap)
        {
            this.source = source;
            this.config = new LanguageBuildConfig(bootstrap);
            this.Getter = Build;
        }

        private bool Build(ILogging logging, out LanguageData result)
        {
            using (var building = new DependencyScope
            {
                logging,
                source,
                config,
                (GrammarReaderProvider p) => p.Reader,
                (GrammarProvider p) => p.Grammar,
                (Grammar g) => g.Options,
                (Grammar g) => g.Reports,
                (ScannerAutomataProvider p) => p.Tdfa,
                (ScannerAmbiguityProvider p) => p.Ambiguities,
                (ParserAutomataProvider p) => p.Dfa,
                (ParserTableProvider p) => p.LrParserTable,
                (RuntimeGrammarProvider p) => p.Outcome,
                (LanguageDataInstanceProvider p) => p.Data
            })
            {
                if (building.HasNo<IGrammarReader>()
                 || building.HasNo<ITdfaData>()
                 || building.HasNo<ILrDfa>())
                {
                    result = null;
                    return false;
                }

                result = building.Get<LanguageData>();

                using (var reporting = new DependencyScope(parent: building)
                {
                    { typeof(IReportData), typeof(ReportData) }
                })
                {
                    reporting.Ensure<LanguageBuildReports>();
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as LanguageDataProvider;
            return casted != null
                && object.Equals(casted.source, source);
        }

        public override int GetHashCode()
        {
            return source.GetHashCode();
        }

        public override string ToString()
        {
            return "LanguageData for " + source.FullLanguageName;
        }
    }
}
