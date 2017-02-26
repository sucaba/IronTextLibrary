using IronText.Algorithm;
using IronText.Automata.Regular;
using IronText.Extensibility;
using IronText.Reflection;
using IronText.Reporting;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    internal class LanguageData
    {
        public Grammar          Grammar             { get; set; }

        public ParserInstruction[] Instructions     { get; set; }

        public RuntimeGrammar   RuntimeGrammar      { get; set; }

        public ParserRuntime    TargetParserRuntime { get; set; }

        public ITable<ParserDecision> ParserDecisionTable   { get; set; }

        public ITable<int>      ParserStartTable   { get; set; }

        public int[]            StateToToken        { get; set; }

        public int[]            TokenComplexity     { get; set; }

        public StackSemanticBinding[]  SemanticBindings  { get; set; }

        public int[]            MatchActionToToken  { get; set; }

        public ITdfaData        ScannerTdfa { get; set; }
    }
}