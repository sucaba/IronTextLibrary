using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Compiler.Analysis;
using IronText.Reflection;
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
            ParserRuntimeDesignator         runtimeDesignator)
        {
            Data = new LanguageData
            {
                TargetParserRuntime       = runtimeDesignator.ActualRuntime,
                Grammar                   = grammar,
                RuntimeGrammar            = runtimeGrammar,
                TokenComplexity           = analysis.GetTokenComplexity(),
                StateToToken              = parserDfa.GetStateToSymbolTable(),
                ParserActionTable         = lrTable.ParserActionTable,
                ParserActionStartTable    = instructionProvider.StartTable,
                ParserConflicts           = lrTable.Conflicts,
                MatchActionToToken        = actionToTokenProvider.ActionToToken,
                ScannerTdfa               = scannerTdfa,
                SemanticBindings          = semanticBindingsProvider.Bindings.ToArray(),
            };
        }

        public LanguageData Data { get; }
    }
}
