using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Compiler.Analysis;
using IronText.Reflection;
using IronText.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.MetadataCompiler
{
    class LanguageDataInstanceProvider
    {
        public LanguageDataInstanceProvider(
            Grammar grammar,
            GrammarAnalysis analysis,
            ITdfaData scannerTdfa,
            MatchActionToTokenTableProvider actionToTokenProvider,
            ILrDfa parserDfa,
            ILrParserTable lrTable,
            RuntimeGrammar runtimeGrammar,
            SemanticBindingProvider semanticBindingsProvider,
            ParserBytecodeProvider instructionProvider)
        {
            this.Data = new LanguageData
            {
                TargetParserRuntime       = lrTable.TargetRuntime,
                Grammar                   = grammar,
                RuntimeGrammar            = runtimeGrammar,
                TokenComplexity           = analysis.GetTokenComplexity(),
                StateToToken              = parserDfa.GetStateToSymbolTable(),
                ParserActionTable         = lrTable.GetParserActionTable(),
                ParserActionStartTable    = instructionProvider.StartTable,
                ParserConflictActionTable = lrTable.GetConflictActionTable(),
                MatchActionToToken        = actionToTokenProvider.ActionToToken,
                ScannerTdfa               = scannerTdfa,
                SemanticBindings          = semanticBindingsProvider.Bindings.ToArray(),
            };
        }

        public LanguageData Data { get; }
    }
}
