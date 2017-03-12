using IronText.Automata.TurnPlanning;
using IronText.Common;
using IronText.Reflection;
using IronText.Reporting;
using IronText.Runtime;
using System.IO;

namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnBasedReportData : IReportData
    {
        private readonly LanguageData                     data;
        private readonly ShrodingerTokenDfaProvider       dfaProvider;
        private readonly Indexer<ShrodingerTokenDfaState> dfaStateIndexer;

        public TurnBasedReportData(
            ILanguageSource            source,
            LanguageData               data,
            ShrodingerTokenDfaProvider dfaProvider,
            ReturnLookaheadProvider    returnLaProvider,
            Indexer<ShrodingerTokenDfaState> stateIndexer,
            TurnBasedNameProvider      turnNameProvider)
        {
            this.Source      = source;
            this.data        = data;
            this.dfaProvider = dfaProvider;
            this.dfaStateIndexer = stateIndexer;
            this.ParserAutomata = new TurnBasedParserAutomata(
                                    dfaProvider,
                                    returnLaProvider,
                                    dfaStateIndexer,
                                    data.Grammar,
                                    turnNameProvider);
        }

        public string DestinationDirectory =>
            (Source as IReportDestinationHint)
                ?.OutputDirectory
                ?? Path.GetTempPath();

        public ILanguageSource Source { get; }

        public Grammar         Grammar => data.Grammar;

        public IParserAutomata ParserAutomata { get; }

        public IScannerAutomata   GetScannerAutomata() => data.ScannerTdfa;

        public ISemanticBinding[] SemanticBindings => data.SemanticBindings;
    }
}
