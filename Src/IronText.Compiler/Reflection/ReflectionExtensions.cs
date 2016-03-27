using IronText.MetadataCompiler;
using IronText.Runtime;
using IronText.Runtime.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public static class ReflectionExtensions
    {
        public static RuntimeGrammar ToRuntime(this Grammar grammar)
        {
            return ToRuntime(grammar, null, null);
        }

        public static RuntimeGrammar ToRuntime(
            this Grammar grammar,
            RuntimeFormula[][] stateToFormulas,
            RuntimeFormula[][] productionToFormulas)
        {
            IRuntimeNullableFirstTables tables = new NullableFirstTables(grammar);
            var tokenIsNullable     = tables.TokenToNullable;

            var tokenIsTerminal    = grammar.Symbols.CreateCompatibleArray(s => s.IsTerminal);
            var tokenCategories    = grammar.Symbols.CreateCompatibleArray(s => s.Categories);
            var tokenNames         = grammar.Symbols.CreateCompatibleArray(s => s.Name);
            var runtimeProductions = grammar.Productions.CreateCompatibleArray(ToRuntime);

            return new RuntimeGrammar(
                        tokenNames,
                        tokenCategories,
                        tokenIsNullable,
                        tokenIsTerminal,
                        runtimeProductions,
                        stateToFormulas,
                        productionToFormulas);
        }

        public static RuntimeProduction ToRuntime(this Production prod)
        {
            return new RuntimeProduction(
                    prod.Index,
                    prod.OutcomeToken,
                    prod.InputTokens.ToArray());
        }
    }
}
