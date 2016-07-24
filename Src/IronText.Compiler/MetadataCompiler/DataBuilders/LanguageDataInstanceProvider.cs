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
            ParserBytecodeProvider          instructionProvider)
        {
            Data = new LanguageData
            {
                TargetParserRuntime       = lrTable.TargetRuntime,
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
