using IronText.Automata.TurnPlanning;
using IronText.Reflection;
using IronText.Reporting;
using IronText.Runtime;
using System.IO;

/*
namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnDfaReportData : IReportData
    {
        private readonly LanguageData               data;
        private readonly ShrodingerTokenDfaProvider dfaProvider;
        private TurnParserAutomata parserAutomata;

        public TurnDfaReportData(
            ILanguageSource            source,
            LanguageData               data,
            ShrodingerTokenDfaProvider dfaProvider)
        {
            this.Source      = source;
            this.data        = data;
            this.dfaProvider = dfaProvider;
        }

        public string DestinationDirectory =>
            (Source as IReportDestinationHint)
                ?.OutputDirectory
                ?? Path.GetTempPath();

        public ILanguageSource Source { get; }

        public Grammar         Grammar => data.Grammar;

        public IParserAutomata ParserAutomata =>
            parserAutomata ?? (parserAutomata = new TurnParserAutomata(dfaProvider));

        public IScannerAutomata   GetScannerAutomata() => data.ScannerTdfa;

        public ISemanticBinding[] SemanticBindings => data.SemanticBindings;
    }
}
*/
