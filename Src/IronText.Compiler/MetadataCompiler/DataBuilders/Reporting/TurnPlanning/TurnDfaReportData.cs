using IronText.Automata.TurnPlanning;
using IronText.Common;
using IronText.Reflection;
using IronText.Reporting;
using IronText.Runtime;
using System.IO;

namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnDfaReportData : IReportData
    {
        private readonly LanguageData                     data;
        private readonly ShrodingerTokenDfaProvider       dfaProvider;
        private TurnParserAutomata                        parserAutomata;
        private readonly Indexer<ShrodingerTokenDfaState> dfaStateIndexer;

        public TurnDfaReportData(
            ILanguageSource            source,
            LanguageData               data,
            ShrodingerTokenDfaProvider dfaProvider,
            Indexer<ShrodingerTokenDfaState> stateIndexer)
        {
            this.Source      = source;
            this.data        = data;
            this.dfaProvider = dfaProvider;
            this.dfaStateIndexer = stateIndexer;
        }

        public string DestinationDirectory =>
            (Source as IReportDestinationHint)
                ?.OutputDirectory
                ?? Path.GetTempPath();

        public ILanguageSource Source { get; }

        public Grammar         Grammar => data.Grammar;

        public IParserAutomata ParserAutomata =>
            parserAutomata ?? (parserAutomata = new TurnParserAutomata(dfaProvider, dfaStateIndexer));

        public IScannerAutomata   GetScannerAutomata() => data.ScannerTdfa;

        public ISemanticBinding[] SemanticBindings => data.SemanticBindings;
    }
}
