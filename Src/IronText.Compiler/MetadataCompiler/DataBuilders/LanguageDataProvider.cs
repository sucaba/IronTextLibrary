using System;
using IronText.Automata.Lalr1;
using IronText.Build;
using IronText.Compiler.Analysis;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;
using IronText.DI;

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
            using (var di = new DependencyScope
                            {
                                logging,
                                source,
                                config,
                                (GrammarReaderProvider p) => p.Reader,
                                (GrammarProvider p) => p.Grammar,
                                (Grammar p) => p.Options,
                                (ScannerAutomataProvider p) => p.Ambiguities,
                                (ScannerAutomataProvider p) => p.Tdfa,
                                (ParserAutomataProvider p) => p.Dfa,
                                (ParserTableProvider p) => p.LrParserTable,
                                (RuntimeGrammarProvider p) => p.Outcome,
                                (LanguageDataInstanceProvider p) => p.Data
                            })
            {
                if (di.Has((IGrammarReader r) => r == null)
                    || di.Has((ScannerAutomataProvider p) => !p.Success)
                    || di.Has((ILrDfa lrDfa) => lrDfa == null))
                {
                    result = null;
                    return false;
                }

                result = di.Get<LanguageData>();

                using (var nestedDi = di.Nest())
                {
                    nestedDi.Add<ReportData>();
                    nestedDi.Get<LanguageBuildReports>();
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
