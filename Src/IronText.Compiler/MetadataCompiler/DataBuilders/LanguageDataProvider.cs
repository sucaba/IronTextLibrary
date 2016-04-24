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
        private readonly ILanguageSource source;
        private readonly bool           bootstrap;
        private ILogging                logging;

        public LanguageDataProvider(ILanguageSource source, bool bootstrap)
        {
            this.source    = source;
            this.bootstrap = bootstrap;
            this.Getter    = Build;
        }

        private bool Build(ILogging logging, out LanguageData result)
        {
            this.logging = logging;

            var readerType = Type.GetType(source.GrammarReaderTypeName);
            if (readerType == null)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = string.Format(
                                    "Unable to find grammar reader '{0}' for language '{1}'",
                                    source.GrammarReaderTypeName,
                                    source.LanguageName),
                        Origin = source.GrammarOrigin
                    });
                result = null;
                return false;
            }

            using (var di = new DependencyScope())
            {
                di.Register(logging);
                di.Register(readerType);
                di.Register(source);
                di.Register(new LanguageBuildConfig(bootstrap));
                di.Register((GrammarProvider p) => p.Grammar);
                di.Register((Grammar p) => p.Options);
                di.Register((ScannerTdfaProvider p) => p.Ambiguities);
                di.Register((ScannerTdfaProvider p) => p.Tdfa);
                di.Register((ParserDfaProvider p) => p.Dfa);
                di.Register((ParserTableProvider p) => p.LrParserTable);
                di.Register((RuntimeGrammarProvider p) => p.Outcome);
                di.Register((LanguageDataInstanceProvider p) => p.Data);

                var scannerTdfaProvider = di.Resolve<ScannerTdfaProvider>();
                if (!scannerTdfaProvider.Success)
                {
                    result = null;
                    return false;
                }

                ILrDfa parserDfa = di.Resolve<ILrDfa>();
                if (parserDfa == null)
                {
                    result = null;
                    return false;
                }

                result = di.Resolve<LanguageData>();

                using (var nestedDi = di.Nest())
                {
                    nestedDi.Register<IReportData,ReportData>();
                    nestedDi.Resolve<LanguageBuildReports>();
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
