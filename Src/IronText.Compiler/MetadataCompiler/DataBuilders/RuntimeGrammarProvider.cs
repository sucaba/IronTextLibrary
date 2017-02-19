using IronText.Reflection;
using IronText.Runtime;
using IronText.Automata;

namespace IronText.MetadataCompiler
{
    internal class RuntimeGrammarProvider
    {
        public RuntimeGrammarProvider(
            Grammar                  grammar,
            RuntimeSemanticsProvider semanticsProvider,
            IParserBytecodeProvider  bytecodeProvider,
            NullableFirstTables      tables)
        {
            var tokenIsNullable     = tables.TokenToNullable;

            var tokenIsTerminal    = grammar.Symbols.CreateCompatibleArray(s => s.IsTerminal);
            var tokenCategories    = grammar.Symbols.CreateCompatibleArray(s => s.Categories);
            var tokenNames         = grammar.Symbols.CreateCompatibleArray(s => s.Name);
            var runtimeProductions = grammar.Productions.CreateCompatibleArray(ToRuntime);

            Outcome = new RuntimeGrammar(
                        tokenNames,
                        tokenCategories,
                        tokenIsNullable,
                        tokenIsTerminal,
                        runtimeProductions,
                        semanticsProvider?.StateToFormulas,
                        semanticsProvider?.ProductionToFormulas,
                        bytecodeProvider?.Instructions);
        }

        public RuntimeGrammar Outcome { get; }

        RuntimeProduction ToRuntime(Production production) =>
            new RuntimeProduction(
                production.Index,
                production.Outcome.Index,
                production.InputTokens);
    }
}
