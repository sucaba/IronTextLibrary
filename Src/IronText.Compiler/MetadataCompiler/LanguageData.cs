using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Compiler.Analysis;
using IronText.Extensibility;
using IronText.Reflection;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    internal class LanguageData
    {
        public Grammar                  Grammar { get; set; }

        public GrammarAnalysis          GrammarAnalysis { get; set; }

        public bool                     IsDeterministic;
        public DotState[]               ParserStates;

        public ProductionContextLink[]  LocalParseContexts;
        public ITable<int>              ParserActionTable;
        public int[]                    ParserConflictActionTable;
        public int[]                    StateToSymbolTable;
    }
}
