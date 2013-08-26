using System;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;

namespace IronText.Extensibility
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    public class LanguageData
    {
        public LanguageName           LanguageName;

        public bool                   IsAmbiguous;
        public Type                   RootContextType;
        public BnfGrammar             Grammar;

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
