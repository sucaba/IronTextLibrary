using System;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;
using System.Collections.ObjectModel;
using IronText.Extensibility;
using System.Linq;
using IronText.Automata.Regular;
using IronText.Automata.Lalr1;
using IronText.Framework.Reflection;
using IronText.Compiler;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    internal class LanguageData : IReportData
    {
        public LanguageName Name { get; set; }

        public EbnfGrammar Grammar { get; set; }

        public EbnfGrammarAnalysis GrammarAnalysis { get; set; }

        public int TokenCount { get { return Grammar.Symbols.Count; } }

        public bool                   IsDeterministic;
        public Type                   RootContextType;

        public DotState[]             ParserStates;

        internal ITokenRefResolver    TokenRefResolver; 

        // Rule ID -> ActionBuilder
        public ProductionActionBuilder[][] RuleActionBuilders;
        public MergeRule[]            MergeRules;
        public SwitchRule[]           SwitchRules;
        public LocalParseContext[]    LocalParseContexts;
        
        public ScanMode[]             ScanModes;
        public Dictionary<Type, ITdfaData>  ScanModeTypeToDfa;
        public IIntMap<int>           AmbTokenToMainToken;

        public ITable<int>            ParserActionTable;
        public int[]                  ParserConflictActionTable;
        public int[]                  StateToSymbolTable;
        public ParserConflictInfo[]   ParserConflicts;

        string IReportData.DestinationDirectory { get { return Name.SourceAssemblyDirectory; } }

        ReadOnlyCollection<ScanMode> IReportData.ScanModes
        {
            get { return new ReadOnlyCollection<ScanMode>(ScanModes); }
        }

        IScannerAutomata IReportData.GetScanModeDfa(Type scanModeType)
        {
            return ScanModeTypeToDfa[scanModeType];
        }

        IParserAutomata IReportData.ParserAutomata
        {
            get { return new ParserAutomata(this); }
        }
    }
}
