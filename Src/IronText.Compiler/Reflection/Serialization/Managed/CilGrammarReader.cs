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

        private static Grammar BuildGrammar(CilGrammar definition)
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
                    ActionContextRef contextRef = CreateActionContextRef(cilProduction.Context);
                    production.Actions.Add(new ProductionAction(pattern.Length, contextRef));
                }

                var action = production.Actions[0];
                action.Joint.Add(cilProduction);

                production.ExplicitPrecedence = cilProduction.Precedence;
            }

            // Create conditions to allow referencing them from matchers
            foreach (CilCondition cilCondition in definition.Conditions)
            {
                var cond = CreateCondtion(result, cilCondition);
                result.Conditions.Add(cond);
            }

            // Create matchers
            foreach (CilCondition cilCondition in definition.Conditions)
            {
                var condition = ConditionFromType(result, cilCondition.ConditionType);

                foreach (var cilMatcher in cilCondition.Matchers)
                {
                    SymbolBase outcome = GetMatcherOutcomeSymbol(result, symbolResolver, cilMatcher.MainOutcome, cilMatcher.AllOutcomes);

                    var matcher = new Matcher(
                        cilMatcher.Pattern,
                        outcome,
#if false
                        context: CreateActionContextRef(cilMatcher.Context),
#endif
                        nextCondition: ConditionFromType(result, cilMatcher.NextConditionType),
                        disambiguation: cilMatcher.Disambiguation);
                    matcher.Joint.Add(cilMatcher);

                    condition.Matchers.Add(matcher);
                }
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

        private static ActionContextRef CreateActionContextRef(CilContextRef cilContext)
        {
            ActionContextRef result;

            if (cilContext == CilContextRef.None)
            {
                result = ActionContextRef.None;
            }
            else
            {
                result = new ActionContextRef(cilContext.UniqueName);
            }

            return result;
        }

        private static Condition ConditionFromType(Grammar grammar, Type type)
        {
            if (type == null)
            {
                return null;
            }

            foreach (var cond in grammar.Conditions)
            {
                var binding = cond.Joint.The<CilCondition>();
                if (binding.ConditionType == type)
                {
                    return cond;
                }
            }

            throw new InvalidOperationException("Undefined condition: " + type.FullName);
        }

        private static Condition CreateCondtion(Grammar grammar, CilCondition cilCondition)
        {
            var result = new Condition(cilCondition.ConditionType.FullName)
            {
                Joint = { cilCondition }
            };

            InitContextProvider(grammar, cilCondition.ContextProvider, result.ContextProvider);
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
            ActionContextProvider    provider)
        {
            provider.Joint.Add(cilProvider);

            foreach (var cilContext in cilProvider.Contexts)
            {
                ActionContext context;
                if (grammar.Contexts.FindOrAdd(cilContext.UniqueName, out context))
                {
                    context.Joint.Add(cilContext);
                }

                provider.Add(context);
            }
        }
    }
}
