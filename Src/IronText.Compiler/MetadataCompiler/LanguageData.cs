using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Compiler.Analysis;
using IronText.Extensibility;
using IronText.Reflection;
using IronText.Runtime;
using IronText.Runtime.Semantics;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    internal class LanguageData
    {
        public Grammar          Grammar             { get; set; }

        public RuntimeGrammar   RuntimeGrammar      { get; set; }

        public GrammarAnalysis  Analysis            { get; set; }

        public bool             IsDeterministic     { get; set; }

        public ITable<int>      ParserActionTable   { get; set; }

        public int[]            ParserConflictActionTable { get; set; }

        public int[]            StateToToken        { get; set; }

        public int[]            TokenComplexity     { get; set; }

        public StackSemanticBinding[]  SemanticBindings  { get; set; }

        public int[]            MatchActionToToken  { get; set; }

        public ITdfaData        ScannerTdfa { get; set; }
    }
}