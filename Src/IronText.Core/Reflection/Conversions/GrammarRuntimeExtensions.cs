using IronText.Misc;
using IronText.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal static class GrammarRuntimeExtensions
    {

        public static Grammar GetGrammar(this Interpreter interpreter) 
        { 
            return interpreter.LanguageRuntime.GetGrammar();
        }

        public static Grammar GetGrammar(this ILanguageRuntime language)
        {
            var internals = (ILanguageInternalRuntime)language;
            return (Grammar)internals.GetSourceGrammar();
        }

        public static RuntimeGrammar ToRuntime(this Grammar grammar)
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
                        runtimeProductions);
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
