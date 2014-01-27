﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Reporting;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    public interface ICilGrammarBuilder
    {
        IEnumerable<ReportBuilder> ReportBuilders { get; }

        EbnfGrammar Build(LanguageName languageName, ILogging logging);
    }

    class CilGrammarBuilder : ICilGrammarBuilder
    {
        private ILogging logging;
        private readonly List<ReportBuilder> _reportBuilders = new List<ReportBuilder>();

        public IEnumerable<ReportBuilder> ReportBuilders
        {
            get { return _reportBuilders; }
        }

        public EbnfGrammar Build(LanguageName languageName, ILogging logging)
        {
            this.logging = logging;

            LanguageDefinition definition = null;

            logging.WithTimeLogging(
                languageName.Name,
                languageName.DefinitionType,
                () =>
                {
                    definition = new LanguageDefinition(languageName.DefinitionType, logging);
                },
                "parsing language definition");
                
            if (definition == null || !definition.IsValid)
            {
                return null;
            }

            _reportBuilders.AddRange(definition.ReportBuilders);

            var tokenResolver = definition.SymbolResolver;

            var grammar = BuildGrammar(definition);
//            var inliner = new ProductionInliner(grammar);
//            grammar = inliner.Inline();
            return grammar;
        }

        private static EbnfGrammar BuildGrammar(LanguageDefinition definition)
        {
            var result = new EbnfGrammar();

            // Define grammar tokens
            var tokenResolver = definition.SymbolResolver;

            foreach (var cilSymbol in tokenResolver.Definitions)
            {
                Symbol symbol;
                if (cilSymbol.Type == typeof(Exception))
                {
                    cilSymbol.Symbol = symbol = (Symbol)result.Symbols[EbnfGrammar.Error];
                    symbol.Joint.Add(
                        new CilSymbol 
                        { 
                            Type = typeof(Exception),
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
                var symbol = tokenResolver.GetSymbol(feature.SymbolRef);
                symbol.Precedence = feature.Value;
            }

            foreach (CilSymbolFeature<CilContextProvider> feature in definition.ContextProviders)
            {
                var symbol = tokenResolver.GetSymbol(feature.SymbolRef);
                if (symbol != null)
                {
                    foreach (var contextType in feature.Value.GetAllContextTypes())
                    {
                        ProductionContext context;
                        if (result.ProductionContexts.FindOrAdd(contextType.AssemblyQualifiedName, out context))
                        {
                            context.Joint.Add(new CilContextConsumer(contextType));
                        }

                        symbol.ProvidedContexts.Add(context);
                    }

                    symbol.Joint.Add(feature.Value);
                }
            }

            result.Start = tokenResolver.GetSymbol(definition.Start);

            // Define grammar rules
            foreach (var cilProduction in definition.Productions)
            {
                Symbol outcome = tokenResolver.GetSymbol(cilProduction.Left);
                var pattern = Array.ConvertAll(cilProduction.Parts, tokenResolver.GetSymbol);

                // Try to find existing rules whith same token-signature
                Production production;
                if (result.Productions.FindOrAdd(outcome, pattern, out production))
                {
                    ProductionContext context;

                    var contextType = cilProduction.InstanceDeclaringType;
                    if (contextType == null)
                    {
                        context = ProductionContext.Global;
                    }
                    else if (result.ProductionContexts.FindOrAdd(contextType.AssemblyQualifiedName, out context))
                    {
                        context.Joint.Add(new CilContextConsumer(contextType));
                    }

                    production.Action = new SimpleProductionAction(pattern.Length, context);
                }

                var action = (SimpleProductionAction)production.Action;
                action.Joint.Add(cilProduction);

                production.ExplicitPrecedence = cilProduction.Precedence;

                cilProduction.Index = production.Index;
            }

            // Create conditions to allow referencing them from scan productions
            foreach (CilScanCondition cilCondition in definition.ScanConditions)
            {
                CreateCondtion(result, cilCondition);
            }

            // Create scan productions
            foreach (CilScanCondition cilCondition in definition.ScanConditions)
            {
                var condition = ConditionFromType(result, cilCondition.ConditionType);

                foreach (var scanProd in cilCondition.Productions)
                {
                    SymbolBase outcome = GetScanProductionOutcomeSymbol(result, tokenResolver, scanProd.MainOutcome, scanProd.AllOutcomes);

                    var scanProduction = new ScanProduction(
                        scanProd.Pattern,
                        outcome,
                        nextCondition: ConditionFromType(result, scanProd.NextModeType),
                        disambiguation: scanProd.Disambiguation);
                    scanProduction.Joint.Add(scanProd);

                    condition.ScanProductions.Add(scanProduction);
                }
            }

            foreach (var cilMerger in definition.Mergers)
            {
                var symbol  = tokenResolver.GetSymbol(cilMerger.Symbol);
                var merger = new Merger(symbol) { Joint = { cilMerger } };
                result.Mergers.Add(merger);
            }

            return result;
        }

        private static ScanCondition ConditionFromType(EbnfGrammar grammar, Type type)
        {
            if (type == null)
            {
                return null;
            }

            foreach (var cond in grammar.ScanConditions)
            {
                var binding = cond.Joint.The<CilScanCondition>();
                if (binding.ConditionType == type)
                {
                    return cond;
                }
            }

            throw new InvalidOperationException("Undefined condition: " + type.FullName);
        }

        private static ScanCondition CreateCondtion(EbnfGrammar grammar, CilScanCondition cilCondition)
        {
            var result = new ScanCondition(cilCondition.ConditionType.FullName)
            {
                Joint = { cilCondition }
            };

            grammar.ScanConditions.Add(result);

            return result;
        }

        private static SymbolBase GetScanProductionOutcomeSymbol(
            EbnfGrammar                 grammar,
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
