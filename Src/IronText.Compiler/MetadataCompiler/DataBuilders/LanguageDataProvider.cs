using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Build;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Misc;
using System.Text;
using IronText.Algorithm;
using IronText.Reflection;
using IronText.Compiler;
using IronText.Compiler.Analysis;
using IronText.Analysis;
using IronText.Logging;
using IronText.Reporting;
using IronText.Runtime;
using IronText.Reflection.Managed;

namespace IronText.MetadataCompiler
{
    internal class LanguageDataProvider : ResourceGetter<LanguageData>
    {
        private readonly LanguageName languageName;
        private readonly bool         bootstrap;
        private ILogging              logging;

        public LanguageDataProvider(LanguageName name, bool bootstrap)
        {
            this.languageName = name;
            this.bootstrap = bootstrap;
            Getter = Build;
        }

        private bool Build(ILogging logging, out LanguageData result)
        {
            result = new LanguageData();

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
                result = null;
                return false;
            }

            var reportBuilders = new List<ReportBuilder>(definition.ReportBuilders);

            var tokenResolver = definition.SymbolResolver;

            var grammar = BuildGrammar(definition);
//            var inliner = new ProductionInliner(grammar);
//            grammar = inliner.Inline();

            if (!bootstrap)
            {
                var conditionTypeToDfa = CompileScannerTdfas(grammar);
                if (conditionTypeToDfa == null)
                {
                    return false;
                }
                
                result.ScanModeTypeToDfa = conditionTypeToDfa;
            }

            // Build parsing tables
            ILrDfa parserDfa = null;

            var grammarAnalysis = new EbnfGrammarAnalysis(grammar);
            logging.WithTimeLogging(
                languageName.Name,
                languageName.DefinitionType,
                () =>
                {
                    parserDfa = new Lalr1Dfa(grammarAnalysis, LrTableOptimizations.Default);
                },
                "building LALR1 DFA");

            if (parserDfa == null)
            {
                result = null;
                return false;
            }

            var flags = Attributes.First<LanguageAttribute>(languageName.DefinitionType).Flags;
            var lrTable = new ConfigurableLrTable(parserDfa, flags);
            if (!lrTable.ComplyWithConfiguration)
            {
                reportBuilders.Add(
                    reportData =>
                    {
                        var messageBuilder = new ConflictMessageBuilder(reportData);
                        messageBuilder.Write(logging);
                    });
            }

            var localParseContexts = CollectLocalContexts(grammar, parserDfa, definition.Productions);

            // Prepare language data for the language assembly generation
            result.Name                = languageName;
            result.IsDeterministic     = !lrTable.RequiresGlr;
            result.RootContextType     = languageName.DefinitionType;
            result.Grammar             = grammar;
            result.GrammarAnalysis     = grammarAnalysis;
            result.ParserStates        = parserDfa.States;
            result.StateToSymbolTable  = parserDfa.GetStateToSymbolTable();
            result.ParserActionTable   = lrTable.GetParserActionTable();
            result.ParserConflictActionTable = lrTable.GetConflictActionTable();
            result.ParserConflicts     = lrTable.Conflicts;

            result.LocalParseContexts  = localParseContexts.ToArray();

            if (!bootstrap)
            {
                foreach (var reportBuilder in reportBuilders)
                {
                    reportBuilder(result);
                }
            }

            return true;
        }

        private Dictionary<Type, ITdfaData> CompileScannerTdfas(EbnfGrammar grammar)
        {
            var result = new Dictionary<Type,ITdfaData>();

            var tokenSet = new BitSetType(grammar.Symbols.Count);

            IScanAmbiguityResolver scanAmbiguityResolver
                                = new ScanAmbiguityResolver(
                                        tokenSet,
                                        grammar.ScanProductions.Count);

            foreach (var condition in grammar.ScanConditions)
            {
                var conditionBinding = condition.Joint.The<CilScanCondition>();

                ITdfaData tdfaData;
                if (!CompileTdfa(logging, condition, out tdfaData))
                {
                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Member = languageName.DefinitionType,
                            Message = string.Format(
                                        "Unable to create scanner for '{0}' language.",
                                        languageName.DefinitionType)
                        });

                    return null;
                }

                result[conditionBinding.ConditionType] = tdfaData;

                // For each action store information about produced tokens
                foreach (var scanProduction in condition.ScanProductions)
                {
                    scanAmbiguityResolver.RegisterAction(scanProduction);
                }

                // For each 'ambiguous scanner state' deduce all tokens
                // which can be produced in this state.
                foreach (var state in tdfaData.EnumerateStates())
                {
                    scanAmbiguityResolver.RegisterState(state);
                }
            }

            scanAmbiguityResolver.DefineAmbiguities(grammar);

            return result;
        }

        private static bool CompileTdfa(ILogging logging, ScanCondition condition, out ITdfaData tdfaData)
        {
            var descr = ScannerDescriptor.FromScanRules(
                                        condition.Name,
                                        condition.ScanProductions,
                                        logging);

            var literalToAction = new Dictionary<string, int>();
            var ast = descr.MakeAst(literalToAction);
            if (ast == null)
            {
                tdfaData = null;
                return false;
            }

            var regTree = new RegularTree(ast);
            tdfaData = new RegularToTdfaAlgorithm(regTree, literalToAction).Data;

            return true;
        }

        private static EbnfGrammar BuildGrammar(LanguageDefinition definition)
        {
            var result = new EbnfGrammar();

            // Define grammar tokens
            var tokenResolver = definition.SymbolResolver;

            foreach (var cilSymbol in tokenResolver.Definitions)
            {
                Symbol symbol;
                if (cilSymbol.SymbolType == typeof(Exception))
                {
                    cilSymbol.Symbol = symbol = (Symbol)result.Symbols[EbnfGrammar.Error];
                    symbol.Joint.Add(
                        new CilSymbol 
                        { 
                            SymbolType = typeof(Exception),
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
                var symbol = tokenResolver.GetSymbol(feature.Symbol);
                symbol.Precedence = feature.Value;
            }

            foreach (CilSymbolFeature<CilContextProvider> feature in definition.ContextProviders)
            {
                var symbol = tokenResolver.GetSymbol(feature.Symbol);
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
                    SymbolBase outcome = GetResultSymbol(result, tokenResolver, scanProd.MainOutcome, scanProd.AllOutcomes);

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

        private static SymbolBase GetResultSymbol(
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

        private static List<ProductionContextLink> CollectLocalContexts(
            EbnfGrammar             grammar,
            ILrDfa                  lrDfa,
            IList<CilProduction> allParseRules)
        {
            var result = new List<ProductionContextLink>();

            var states     = lrDfa.States;
            int stateCount = states.Length;

            for (int parentState = 0; parentState != stateCount; ++parentState)
            {
                foreach (var item in states[parentState].Items)
                {
                    if (item.Position == 0 || item.IsReduce)
                    {
                        // Skip items in which local context cannot be provided.
                        continue;
                    }

                    var providingProd = grammar.Productions[item.ProductionId];
                    var provider      = providingProd.Pattern[0];
                    var childSymbol   = providingProd.Pattern[item.Position];

                    foreach (var consumingProd in childSymbol.Productions)
                    {
                        var action = (SimpleProductionAction)consumingProd.Action;

                        if (provider.ProvidedContexts.Contains(action.Context))
                        {
                            result.Add(
                                new ProductionContextLink
                                {
                                    ParentState = parentState,
                                    ContextTokenLookback = item.Position,
                                    Joint = 
                                    {
                                        provider.Joint.The<CilContextProvider>(),
                                        action.Context.Joint.Get<CilContextConsumer>(),
                                    }
                                });
                        }
                    }
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as LanguageDataProvider;
            return casted != null
                && object.Equals(casted.languageName, languageName);
        }

        public override int GetHashCode()
        {
            return languageName.GetHashCode();
        }

        public override string ToString()
        {
            return "LanguageData for " + languageName.DefinitionType.FullName;
        }
    }
}
