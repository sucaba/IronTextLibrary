using System.IO;
using IronText.Automata.Lalr1;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Runtime;

namespace IronText.Reflection.Reporting
{
    class ReportData : IReportData
    {
        private readonly ILanguageSource source;
        private IParserAutomata parserAutomata;
        internal readonly LanguageData data;
        internal readonly ParserConflictInfo[] parserConflicts;
        internal readonly DotState[] parserStates;

        public ReportData(
            ILanguageSource source,
            LanguageData    data,
            ILrParserTable  lrTable,
            ILrDfa          parserDfa)
        {

            this.source          = source;
            this.data            = data;
            this.parserConflicts = lrTable.Conflicts;
            this.parserStates    = parserDfa.States;
        }

        public string DestinationDirectory
        {
            get 
            {
                var hint = source as IReportDestinationHint;
                if (hint == null)
                {
                    return Path.GetTempPath();
                }

                return hint.OutputDirectory;
            } 
        }

        public ILanguageSource Source { get { return source; } }

        public Grammar Grammar { get { return data.Grammar; } }

        public IParserAutomata ParserAutomata
        {
            get { return parserAutomata ?? (parserAutomata = new ParserAutomata(this)); }
        }

        public IScannerAutomata GetScannerAutomata()
        {
            return data.ScannerTdfa;
        }

        public ISemanticBinding[] SemanticBindings
        {
            get { return data.SemanticBindings; }
        }
    }
}
