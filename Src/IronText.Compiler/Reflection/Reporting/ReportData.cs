using System.IO;
using IronText.Automata.Lalr1;
using IronText.MetadataCompiler;
using IronText.Reflection;

namespace IronText.Reflection.Reporting
{
    class ReportData : IReportData
    {
        private readonly IGrammarSource source;
        private IParserAutomata parserAutomata;
        internal readonly LanguageData data;
        internal readonly ParserConflictInfo[] parserConflicts;
        internal readonly DotState[] parserStates;

        internal ReportData(
            IGrammarSource       source,
            LanguageData         data,
            ParserConflictInfo[] parserConflicts,
            DotState[]           parserStates)
        {
            this.source = source;
            this.data = data;
            this.parserConflicts = parserConflicts;
            this.parserStates = parserStates;
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

        public IGrammarSource Source { get { return source; } }

        public Grammar Grammar { get { return data.Grammar; } }

        public IParserAutomata ParserAutomata
        {
            get { return parserAutomata ?? (parserAutomata = new ParserAutomata(this)); }
        }

        public IScannerAutomata GetScannerAutomata()
        {
            return data.Grammar.Joint.The<IScannerAutomata>();
        }
    }
}
