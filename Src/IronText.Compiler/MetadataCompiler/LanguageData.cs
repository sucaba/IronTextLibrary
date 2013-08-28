using System;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;
using System.Collections.ObjectModel;
using IronText.Extensibility;
using System.Linq;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    internal class LanguageData : IReportData
    {
        string IReportData.DestinationDirectory { get { return Name.SourceAssemblyDirectory; } }

        public LanguageName Name { get; set; }

        public BnfGrammar Grammar { get; set; }

        public int TokenCount { get { return Lalr1ParserActionTable.ColumnCount; } }

        ReadOnlyCollection<ScanMode> IReportData.ScanModes
        {
            get { return new ReadOnlyCollection<ScanMode>(ScanModes); }
        }

        int IReportData.ParserStateCount { get { return ParserStates.Length; } }

        ReadOnlyCollection<DotState> IReportData.ParserStates
        {
            get { return new ReadOnlyCollection<DotState>(ParserStates); }
        }

        ITdfaData IReportData.GetScanModeDfa(Type scanModeType)
        {
            return ScanModeTypeToDfa[scanModeType];
        }

        ParserAction IReportData.GetParserAction(int state, int token)
        {
            return ParserAction.Decode(Lalr1ParserActionTable.Get(state, token));
        }

        IEnumerable<ParserAction> IReportData.GetAllParserActions(int state, int token)
        {
            var cell = Lalr1ParserActionTable.Get(state, token);
            var action = ParserAction.Decode(cell);
            if (action != null && action.Kind == ParserActionKind.Conflict)
            {
                for (int i = 0; i != action.Size; ++i)
                {
                    yield return
                        ParserAction.Decode(
                            Lalr1ParserConflictActionTable[action.Value1 + i]);
                }
            }
        }

        ReadOnlyCollection<ParserConflictInfo> IReportData.ParserConflicts
        {
            get { return new ReadOnlyCollection<ParserConflictInfo>(Lalr1Conflicts); }
        }

        IEnumerable<ParserAction> IReportData.GetConflictActions(int conflictIndex, int count)
        {
            for (int i = 0; i != count; ++i)
            {
                yield return ParserAction.Decode(
                    Lalr1ParserConflictActionTable[conflictIndex + i]);
            }
        }

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
    }
}
