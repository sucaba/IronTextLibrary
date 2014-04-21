using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Collections;
using IronText.Framework;
using IronText.Logging;
using IronText.Misc;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Reflection.Reporting;

namespace IronText.Reflection.Managed
{
    class CilGrammarReader : IGrammarReader
    {
        private ILogging logging;

        public Grammar Read(IGrammarSource source, ILogging logging)
        {
            var cilSource = source as CilGrammarSource;
            if (cilSource == null)
            {
                return null;
            }

            this.logging = logging;

            CilGrammar definition = null;

            logging.WithTimeLogging(
                cilSource.LanguageName,
                cilSource.Origin,
                () =>
                {
                    definition = new CilGrammar(cilSource, logging);
                },
                "parsing language definition");
                
            if (definition == null || !definition.IsValid)
            {
                return null;
            }

            var grammar = BuildGrammar(definition);

            grammar.Options = (IronText.Reflection.RuntimeOptions)Attributes.First<LanguageAttribute>(cilSource.DefinitionType).Flags;
            return grammar;
        }

        private Grammar BuildGrammar(CilGrammar definition)
        {
            var result = new Grammar();

            InitContextProvider(
                result,
                definition.GlobalContextProvider,
                result.GlobalContextProvider);

            var symbolResolver = definition.SymbolResolver;

            // Define grammar tokens
            foreach (var cilSymbol in symbolResolver.Definitions)
            {
                Symbol symbol;
                if (cilSymbol.Type == typeof(Exception))
                {
                    cilSymbol.Symbol = symbol = (Symbol)result.Symbols[PredefinedTokens.Error];
                    symbol.Joint.Add(
                        new CilSymbol 
                        { 
                            Type       = typeof(Exception),
                            Symbol     = symbol,
                            Categories = SymbolCategory.DoNotDelete | SymbolCategory.DoNotInsert
                        });
                }
                else
                {
                    symbol = new Symbol(cilSymbol.Name) 
                    {
                        Categories = cilSymbol.Categories,
                        Joint = { cilSymbol } 
                    };
                    result.Symbols.Add(symbol);
                    cilSymbol.Symbol = symbol;
                }
            }

            foreach (CilSymbolFeature<Precedence> feature in definition.Precedence)
            {
                var symbol = symbolResolver.GetSymbol(feature.SymbolRef);
                if (symbol == null)
                {
                    // Precedence specified for a not used symbol
                    continue;
                }

                symbol.Precedence = feature.Value;
            }

            foreach (CilSymbolFeature<CilContextProvider> feature in definition.LocalContextProviders)
            {
                var symbol = symbolResolver.GetSymbol(feature.SymbolRef);
                if (symbol != null)
                {
                    InitContextProvider(result, feature.Value, symbol.LocalContextProvider);
                }
            }

            result.Start = symbolResolver.GetSymbol(definition.Start);

            // Define grammar rules
            foreach (var cilProduction in definition.Productions)
            {
                Symbol outcome = symbolResolver.GetSymbol(cilProduction.Outcome);
                var pattern = Array.ConvertAll(cilProduction.Pattern, symbolResolver.GetSymbol);

                // Try to find existing rules whith same token-signature
                Production production;
                if (result.Productions.FindOrAdd(outcome, pattern, out production))
                {
                    SemanticContextRef contextRef = CreateActionContextRef(cilProduction.Context);
                    production.Actions.Add(new SemanticAction(production.Index, pattern.Length, contextRef));
                }

                var action = production.Actions[0];
                action.Joint.Add(cilProduction);

                production.ExplicitPrecedence = cilProduction.Precedence;
            }

            // Create matchers
            foreach (var cilMatcher in definition.Matchers)
            {
                SymbolBase outcome = GetMatcherOutcomeSymbol(result, symbolResolver, cilMatcher.MainOutcome, cilMatcher.AllOutcomes);

                var matcher = new Matcher(
                    cilMatcher.Pattern,
                    outcome,
                    disambiguation: cilMatcher.Disambiguation);
                matcher.Joint.Add(cilMatcher);

                result.Matchers.Add(matcher);
            }

            foreach (var cilMerger in definition.Mergers)
            {
                var symbol  = symbolResolver.GetSymbol(cilMerger.Symbol);
                var merger = new Merger(symbol) { Joint = { cilMerger } };
                result.Mergers.Add(merger);
            }

            foreach (var report in definition.Reports)
            {
                result.Reports.Add(report);
            }

            return result;
        }

        private static SemanticContextRef CreateActionContextRef(CilContextRef cilContext)
        {
            SemanticContextRef result;

            if (cilContext == CilContextRef.None)
            {
                result = SemanticContextRef.None;
            }
            else
            {
                result = new SemanticContextRef(cilContext.UniqueName);
            }

            return result;
        }

        private static SymbolBase GetMatcherOutcomeSymbol(
            Grammar                   grammar,
            ICilSymbolResolver        symbolResolver,
            CilSymbolRef              mainOutcome,
            IEnumerable<CilSymbolRef> allOutcomes)
        {
            Symbol main = symbolResolver.GetSymbol(mainOutcome);
            Symbol[] all = (from outcome in allOutcomes
                             select symbolResolver.GetSymbol(outcome))
                             .ToArray();

            switch (all.Length)
            {
                case 0:  return null;
                case 1:  return main;
                default: return new AmbiguousSymbol(main, all);
            }
        }

        private static void InitContextProvider(
            Grammar            grammar,
            CilContextProvider cilProvider,
            SemanticContextProvider    provider)
        {
            provider.Joint.Add(cilProvider);

            foreach (var cilContext in cilProvider.Contexts)
            {
                SemanticContext context;
                if (grammar.Contexts.FindOrAdd(cilContext.UniqueName, out context))
                {
                    context.Joint.Add(cilContext);
                }

                provider.Add(context);
            }
        }
    }
}
