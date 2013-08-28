using System;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;
using System.Collections.ObjectModel;

namespace IronText.Extensibility
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    public class LanguageData : ILanguageData
    {
        public LanguageName LanguageName { get; set; }

        public BnfGrammar Grammar { get; set; }

        public int TokenCount { get { return Lalr1ParserActionTable.ColumnCount; } }

        ReadOnlyCollection<DotState> ILanguageData.ParserStates
        {
            get { return new ReadOnlyCollection<DotState>(ParserStates); }
        }

        ParserAction ILanguageData.GetParserAction(int state, int token)
        {
            return ParserAction.Decode(Lalr1ParserActionTable.Get(state, token));
        }

        IEnumerable<ParserAction> ILanguageData.GetAllParserActions(int state, int token)
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

        ReadOnlyCollection<ParserConflictInfo> ILanguageData.GetParserConflicts()
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
                        var item = new ParserConflictInfo
                            {
                                State = state,
                                Token = token,
                            };

                        for (int i = 0; i != action.Size; ++i)
                        {
                            item.Actions.Add(
                                ParserAction.Decode(
                                    Lalr1ParserConflictActionTable[action.Value1 + i]));
                        }

                        resultList.Add(item);
                    }
                }
            }

            return new ReadOnlyCollection<ParserConflictInfo>(resultList);
        }

        IEnumerable<ParserAction> ILanguageData.GetConflictActions(int conflictIndex, int count)
        {
            for (int i = 0; i != count; ++i)
            {
                yield return ParserAction.Decode(
                    Lalr1ParserConflictActionTable[conflictIndex + i]);
            }
        }

        private ParserConflictInfo[] GetParserConflicts(ILanguageData data)
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
                        var item = new ParserConflictInfo
                            {
                                State = state,
                                Token = token,
                            };

                        for (int i = 0; i != action.Size; ++i)
                        {
                            item.Actions.Add(
                                ParserAction.Decode(
                                    Lalr1ParserConflictActionTable[action.Value1 + i]));
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

        public string GetDestinationDirectory()
        {
            string result = LanguageName.SourceAssemblyDirectory;
            return result;
        }
    }
}
