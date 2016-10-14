using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Compiler.Analysis;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class LanguageDataInstanceProvider
    {
        public LanguageDataInstanceProvider(
            Grammar                         grammar,
            GrammarAnalysis                 analysis,
            ITdfaData                       scannerTdfa,
            MatchActionToTokenTableProvider actionToTokenProvider,
            ILrDfa                          parserDfa,
            ILrParserTable                  lrTable,
            RuntimeGrammar                  runtimeGrammar,
            SemanticBindingProvider         semanticBindingsProvider,
            ParserBytecodeProvider          instructionProvider,
            ParserRuntimeDesignator         runtimeDesignator,
            StateToSymbolTableProvider      stateToSymbolTableProvider)
        {
            Data = new LanguageData
            {
                TargetParserRuntime  = runtimeDesignator.ActualRuntime,
                Grammar              = grammar,
                RuntimeGrammar       = runtimeGrammar,
                TokenComplexity      = analysis.GetTokenComplexity(),
                StateToToken         = stateToSymbolTableProvider.Table,
                ParserDecisionTable  = lrTable.DecisionTable,
                ParserStartTable     = instructionProvider.StartTable,
                MatchActionToToken   = actionToTokenProvider.ActionToToken,
                ScannerTdfa          = scannerTdfa,
                SemanticBindings     = semanticBindingsProvider.Bindings.ToArray(),
            };
        }

        public LanguageData Data { get; }
    }
}
