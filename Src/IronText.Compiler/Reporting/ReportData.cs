using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.Reporting
{
    class ReportData : IReportData
    {
        private readonly IGrammarSource source;
        private IParserAutomata parserAutomata;
        internal readonly LanguageData data;
        internal readonly ParserConflictInfo[] parserConflicts;

        internal ReportData(IGrammarSource source, LanguageData data, ParserConflictInfo[] parserConflicts)
        {
            this.source = source;
            this.data = data;
            this.parserConflicts = parserConflicts;
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

        public IGrammarSource Name { get { return source; } }

        public Grammar Grammar { get { return data.Grammar; } }

        public IParserAutomata ParserAutomata
        {
            get { return parserAutomata ?? (parserAutomata = new ParserAutomata(this)); }
        }

        public IScannerAutomata GetScannerAutomata(Condition condition)
        {
            return condition.Joint.The<IScannerAutomata>();
        }
    }
}
