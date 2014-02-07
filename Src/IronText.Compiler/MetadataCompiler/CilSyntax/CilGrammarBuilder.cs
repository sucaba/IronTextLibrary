using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Logging;
using IronText.Misc;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Reporting;
using IronText.Runtime;

namespace IronText.MetadataCompiler.CilSyntax
{
    class CilGrammarBuilder : IGrammarBuilder
    {
        private ILogging logging;
        private readonly List<ReportBuilder> _reportBuilders = new List<ReportBuilder>();

        public IEnumerable<ReportBuilder> ReportBuilders
        {
            get { return _reportBuilders; }
        }

        public Grammar Build(IGrammarSource source, ILogging logging)
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

            _reportBuilders.AddRange(definition.ReportBuilders);

            var grammar = BuildGrammar(definition);
            grammar.Options = (IronText.Reflection.RuntimeOptions)Attributes.First<LanguageAttribute>(cilSource.DefinitionType).Flags;
            return grammar;
        }

        private static Grammar BuildGrammar(CilGrammar definition)
        {
            var result = new Grammar();

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
                    symbol = new Symbol(cilSymbol.Name) { Categories = cilSymbol.Categories, Joint = { cilSymbol } };
                    result.Symbols.Add(symbol);
                    cilSymbol.Symbol = symbol;
                }
            }

            foreach (CilSymbolFeature<Precedence> feature in definition.Precedence)
            {
                var symbol = symbolResolver.GetSymbol(feature.SymbolRef);
                symbol.Precedence = feature.Value;
            }

            foreach (CilSymbolFeature<CilContextProvider> feature in definition.ContextProviders)
            {
                var symbol = symbolResolver.GetSymbol(feature.SymbolRef);
                if (symbol != null)
                {
                    foreach (var contextType in feature.Value.GetAllContextTypes())
                    {
                        ProductionContext context;
                        if (result.Contexts.FindOrAdd(contextType.AssemblyQualifiedName, out context))
                        {
                            context.Joint.Add(new CilContextConsumer(contextType));
                        }

                        symbol.ProvidedContexts.Add(context);
                    }

                    symbol.Joint.Add(feature.Value);
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
                    ProductionContext context;

                    var contextType = cilProduction.ContextType;
                    if (contextType == null)
                    {
                        context = ProductionContext.Global;
                    }
                    else if (result.Contexts.FindOrAdd(contextType.AssemblyQualifiedName, out context))
                    {
                        context.Joint.Add(new CilContextConsumer(contextType));
                    }

                    production.Action = new SimpleProductionAction(pattern.Length, context);
                }

                var action = (SimpleProductionAction)production.Action;
                action.Joint.Add(cilProduction);

                production.ExplicitPrecedence = cilProduction.Precedence;
            }

            // Create conditions to allow referencing them from scan productions
            foreach (CilCondition cilCondition in definition.ScanConditions)
            {
                CreateCondtion(result, cilCondition);
            }

            // Create scan productions
            foreach (CilCondition cilCondition in definition.ScanConditions)
            {
                var condition = ConditionFromType(result, cilCondition.ConditionType);

                foreach (var scanProd in cilCondition.Matchers)
                {
                    SymbolBase outcome = GetScanProductionOutcomeSymbol(result, symbolResolver, scanProd.MainOutcome, scanProd.AllOutcomes);

                    var scanProduction = new Matcher(
                        scanProd.Pattern,
                        outcome,
                        nextCondition: ConditionFromType(result, scanProd.NextConditionType),
                        disambiguation: scanProd.Disambiguation);
                    scanProduction.Joint.Add(scanProd);

                    condition.Matchers.Add(scanProduction);
                }
            }

            foreach (var cilMerger in definition.Mergers)
            {
                var symbol  = symbolResolver.GetSymbol(cilMerger.Symbol);
                var merger = new Merger(symbol) { Joint = { cilMerger } };
                result.Mergers.Add(merger);
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

            grammar.Conditions.Add(result);

            return result;
        }

        private static SymbolBase GetScanProductionOutcomeSymbol(
            Grammar                 grammar,
            ICilSymbolResolver          symbolResolver,
            CilSymbolRef                mainOutcome,
            IEnumerable<CilSymbolRef>   allOutcomes)
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
    }
}
