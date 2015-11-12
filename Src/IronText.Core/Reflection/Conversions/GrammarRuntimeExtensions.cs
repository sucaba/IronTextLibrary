using IronText.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal static class GrammarRuntimeExtensions
    {
        public static RuntimeGrammar ToRuntime(this Grammar grammar)
        {
            IRuntimeNullableFirstTables tables = new NullableFirstTables(grammar);
            var tokenIsNullable     = tables.TokenToNullable;

            var tokenIsTerminal = grammar.Symbols.CreateCompatibleArray(sym => sym.IsTerminal);
            var tokenCategories = grammar.Symbols.CreateCompatibleArray(s => s.Categories);
            var tokenNames      = grammar.Symbols.CreateCompatibleArray(s => s.Name);
            var nonPredefinedTokens = (from s in grammar.Symbols
                                       where !s.IsPredefined
                                       select s.Index)
                                      .ToArray();

            var runtimeProductions = grammar.Productions.CreateCompatibleArray(ToRuntime);

            return new RuntimeGrammar(
                        grammar.Symbols.Count,
                        runtimeProductions,
                        tokenIsNullable,
                        tokenIsTerminal,
                        nonPredefinedTokens,
                        tokenCategories,
                        tokenNames);
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
