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
using IronText.Runtime;

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

            InitSemanticScope(
                result,
                definition.Globals,
                result.Globals);

            var symbolResolver = definition.SymbolResolver;

            // Define grammar tokens
            foreach (var cilSymbol in symbolResolver.Definitions)
            {
                Symbol symbol;
                if (cilSymbol.Type == typeof(Exception))
                {
                    cilSymbol.Symbol = symbol = result.Error;
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

            foreach (CilSymbolFeature<CilSemanticScope> feature in definition.LocalSemanticScopes)
            {
                var symbol = symbolResolver.GetSymbol(feature.SymbolRef);
                if (symbol != null)
                {
                    InitSemanticScope(result, feature.Value, symbol.LocalScope);
                }
            }

            result.Start = symbolResolver.GetSymbol(definition.Start);

            // Define grammar rules
            foreach (var cilProduction in definition.Productions)
            {
                Symbol outcome = symbolResolver.GetSymbol(cilProduction.Outcome);
                var pattern = Array.ConvertAll(cilProduction.Pattern, symbolResolver.GetSymbol);

                // Try to find existing rules with s same signature
                SemanticRef contextRef = CreateActionContextRef(cilProduction.Context);
                Production production = result.Productions.Find(outcome, pattern);
                if (production == null)
                {
                    production = new Production(outcome, pattern, contextRef, cilProduction.Flags);
                    result.Productions.Add(production);
                }

                production.Joint.Add(cilProduction);

                production.ExplicitPrecedence = cilProduction.Precedence;
            }

            // Create matchers
            foreach (var cilMatcher in definition.Matchers)
            {
                ITerminal outcome = GetMatcherOutcomeSymbol(result, symbolResolver, cilMatcher.AllOutcomes);

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

        private static SemanticRef CreateActionContextRef(CilSemanticRef cilContext)
        {
            SemanticRef result;

            if (cilContext == CilSemanticRef.None)
            {
                result = SemanticRef.None;
            }
            else
            {
                result = SemanticRef.ByName(cilContext.UniqueName);
            }

            return result;
        }

        private static ITerminal GetMatcherOutcomeSymbol(
            Grammar                   grammar,
            ICilSymbolResolver        symbolResolver,
            IEnumerable<CilSymbolRef> allOutcomes)
        {
            Symbol[] all = (from outcome in allOutcomes
                             select symbolResolver.GetSymbol(outcome))
                             .ToArray();

            switch (all.Length)
            {
                case 0:  return null;
                case 1:  return all[0];
                default: return new AmbiguousTerminal(all);
            }
        }

        private static void InitSemanticScope(
            Grammar          grammar,
            CilSemanticScope platformScope,
            SemanticScope    logicalScope)
        {
            logicalScope.Joint.Add(platformScope);

            foreach (var refValuePair in platformScope)
             {
                var reference = SemanticRef.ByName(refValuePair.Key.UniqueName);
                if (!logicalScope.Lookup(reference))
                {
                    SemanticValue value = new SemanticValue(title: refValuePair.Key.UniqueName);
                    value.Joint.Add(refValuePair.Value);
                    logicalScope.Add(reference, value);
                }
            }
        }
    }
}
