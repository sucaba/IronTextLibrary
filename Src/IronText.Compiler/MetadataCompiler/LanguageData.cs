using System;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;
using System.Collections.ObjectModel;
using IronText.Extensibility;
using System.Linq;
using IronText.Automata.Regular;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    internal class LanguageData : IReportData
    {
        public LanguageName Name { get; set; }

        public BnfGrammar Grammar { get; set; }

        public BnfGrammarAnalysis GrammarAnalysis { get; set; }

        public int TokenCount { get { return Lalr1ParserActionTable.ColumnCount; } }

        public bool                   IsAmbiguous;
        public Type                   RootContextType;

        public DotState[]             ParserStates;

        internal ITokenRefResolver    TokenRefResolver; 

        // Rule ID -> ActionBuilder
        public GrammarActionBuilder[][] RuleActionBuilders;
        public MergeRule[]            MergeRules;
        public SwitchRule[]           SwitchRules;
        public LocalParseContext[]    LocalParseContexts;
        
        public ScanMode[]             ScanModes;
        public Dictionary<Type, ITdfaData>  ScanModeTypeToDfa;

        public ITable<int>            ParserActionTable;
        public int[]                  ParserConflictActionTable;
        public int[]                  StateToSymbolTable;

        // For reporting
        public ITable<int>            Lalr1ParserActionTable;
        public int[]                  Lalr1ParserConflictActionTable;
        public ParserConflictInfo[]   Lalr1Conflicts;

        private ParserConflictInfo[] GetParserConflicts(IReportData data)
        {
            var resultList = new List<ParserConflictInfo>();
            for (int state = 0; state != ParserStates.Length; ++state)
            {
                for (int token = 0; token != Grammar.TokenCount; ++token)
                {
                    var cell = Lalr1ParserActionTable.Get(state, token);
                    var action = ParserAction.Decode(cell);
                    if (action != null && action.Kind == ParserActionKind.Conflict)
                    {
                        var item = new ParserConflictInfo(state, token);
                        for (int i = 0; i != action.Size; ++i)
                        {
                            item.AddAction(
                                    Lalr1ParserConflictActionTable[action.Value1 + i]);
                        }

                        resultList.Add(item);
                    }
                }
            }

            return resultList.ToArray();
        }

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
