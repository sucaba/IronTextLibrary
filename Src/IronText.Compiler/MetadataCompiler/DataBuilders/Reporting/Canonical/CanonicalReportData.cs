using System.IO;
using IronText.Automata.Lalr1;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Runtime;

namespace IronText.Reporting
{
    class CanonicalReportData : IReportData
    {
        private readonly ILanguageSource source;
        private IParserAutomata parserAutomata;
        internal readonly LanguageData data;
        internal readonly ParserConflictInfo[] parserConflicts;
        internal readonly DotState[] parserStates;

        public CanonicalReportData(
            ILanguageSource      source,
            LanguageData         data,
            ILrParserTable       lrTable,
            ParserConflictInfo[] parserConflicts,
            ILrDfa               parserDfa)
        {
            this.source          = source;
            this.data            = data;
            this.parserConflicts = parserConflicts;
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
            get { return parserAutomata ?? (parserAutomata = new CanonicalParserAutomata(this)); }
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
