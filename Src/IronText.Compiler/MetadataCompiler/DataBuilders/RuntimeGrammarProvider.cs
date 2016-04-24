using IronText.Reflection;
using IronText.Runtime;
using System.Linq;

namespace IronText.MetadataCompiler
{
    internal class RuntimeGrammarProvider
    {
        public RuntimeGrammarProvider(
            Grammar                  grammar,
            RuntimeSemanticsProvider semanticsProvider,
            ParserBytecodeProvider   bytecodeProvider)
        {
            IRuntimeNullableFirstTables tables = new NullableFirstTables(grammar);
            var tokenIsNullable     = tables.TokenToNullable;

            var tokenIsTerminal    = grammar.Symbols.CreateCompatibleArray(s => s.IsTerminal);
            var tokenCategories    = grammar.Symbols.CreateCompatibleArray(s => s.Categories);
            var tokenNames         = grammar.Symbols.CreateCompatibleArray(s => s.Name);
            var runtimeProductions = grammar.Productions.CreateCompatibleArray(ToRuntime);

            this.Outcome = new RuntimeGrammar(
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

        private static RuntimeProduction ToRuntime(Production prod)
        {
            return new RuntimeProduction(
                    prod.Index,
                    prod.OutcomeToken,
                    prod.InputTokens.ToArray());
        }
    }
}
