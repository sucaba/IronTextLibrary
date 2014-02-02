﻿using System;
using System.Collections.Generic;
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
        private readonly CilGrammarSource name;
        private IParserAutomata parserAutomata;
        internal readonly LanguageData data;
        internal readonly ParserConflictInfo[] parserConflicts;

        internal ReportData(CilGrammarSource name, LanguageData data, ParserConflictInfo[] parserConflicts)
        {
            this.name = name;
            this.data = data;
            this.parserConflicts = parserConflicts;
        }

        public string DestinationDirectory
        {
            get { return name.SourceAssemblyDirectory; } 
        }

        public CilGrammarSource Name { get { return name; } }

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
