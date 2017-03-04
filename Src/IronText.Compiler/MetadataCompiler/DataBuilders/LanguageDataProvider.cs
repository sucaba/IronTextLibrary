using IronText.Automata.Lalr1;
using IronText.Build;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reporting;
using IronText.Runtime;
using IronText.DI;
using IronText.Automata.Regular;
using IronText.MetadataCompiler.DataBuilders.Infrastsructure;
using IronText.MetadataCompiler.Analysis;
using IronText.MetadataCompiler.DataBuilders.Reporting;

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
                typeof(LoggingInstantiator),
                (GrammarReaderProvider p) => p.Reader,
                (GrammarProvider p) => p.Grammar,
                (Grammar g) => g.Options,
                (Grammar g) => g.Reports,
                typeof(BuildtimeGrammar),
                (ScannerAutomataProvider p) => p.Tdfa,
                (ScannerAmbiguityProvider p) => p.Ambiguities,
                (Lalr1DfaProvider p) => (ILrDfa)p,
                (ParserConflictProvider p) => p.Conflicts,
                (ParserTableProvider p) => p.LrParserTable,
                (ParserRuntimeDesignator d) => d.ActualRuntime,
                (RuntimeGrammarProvider p) => p.Outcome,
                typeof(ParserBytecodeProviderSelector),
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
                    typeof(ReportDataSelector)
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
