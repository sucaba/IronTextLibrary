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
using IronText.Framework.Reflection;
using IronText.Compiler;
using IronText.Compiler.Analysis;
using IronText.Analysis;
using IronText.Logging;

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

            var tokenResolver = definition.TokenRefResolver;

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

            var localParseContexts = CollectLocalContexts(grammar, parserDfa, definition.ProductionDefs);

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

            result.TokenRefResolver    = tokenResolver;

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
                var conditionBinding = condition.Joint.The<CilScanConditionDef>();

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
            var tokenResolver = definition.TokenRefResolver;

            foreach (var def in tokenResolver.Definitions)
            {
                if (def.TokenType == typeof(Exception))
                {
                    def.Symbol = (Symbol)result.Symbols[EbnfGrammar.Error];
                }
                else
                {
                    var symbol = new Symbol(def.Name) { Categories = def.Categories };
                    result.Symbols.Add(symbol);
                    def.Symbol = symbol;
                }
            }

            foreach (SymbolFeature<Precedence> feature in definition.Precedence)
            {
                var symbol = tokenResolver.GetSymbol(feature.Token);
                symbol.Precedence = feature.Value;
            }

            foreach (SymbolFeature<CilContextProvider> feature in definition.ContextProviders)
            {
                var symbol = tokenResolver.GetSymbol(feature.Token);
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
            foreach (var prodDef in definition.ProductionDefs)
            {
                Symbol outcome = tokenResolver.GetSymbol(prodDef.Left);
                var pattern = Array.ConvertAll(prodDef.Parts, tokenResolver.GetSymbol);

                // Try to find existing rules whith same token-signature
                Production production;
                if (result.Productions.FindOrAdd(outcome, pattern, out production))
                {
                    ProductionContext context;

                    var contextType = prodDef.InstanceDeclaringType;
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
                action.Joint.Add(prodDef);

                production.ExplicitPrecedence = prodDef.Precedence;

                prodDef.Index = production.Index;
            }

            foreach (CilScanCondition scanMode in definition.ScanModes)
            {
                var condition = ConditionFromType(result, scanMode);

                foreach (var scanRule in scanMode.ScanRules)
                {
                    int mainToken = tokenResolver.GetId(scanRule.MainOutcome);
                    int[] tokens = scanRule
                            .AllOutcomes
                            .Select(c => tokenResolver.GetId(c))
                            .ToArray();

                    SymbolBase  outcome = GetResultSymbol(result, tokenResolver, scanRule.MainOutcome, scanRule.AllOutcomes);
                    ScanPattern pattern = CreateScanPattern(scanRule);

                    var scanProduction = new ScanProduction(
                        pattern,
                        outcome,
                        nextCondition: ConditionFromType(result, scanRule.NextModeType),
                        disambiguation: scanRule.Disambiguation);
                    scanProduction.Joint.Add(
                        new CilScanProductionDef(scanRule.DefiningMethod, scanRule.Builder));

                    condition.ScanProductions.Add(scanProduction);
                }
            }

            foreach (var mergerDef in definition.MergerDefs)
            {
                int token = tokenResolver.GetId(mergerDef.Token);
                var symbol = (Symbol)result.Symbols[token];

                var merger = new Merger(symbol) { Joint = { mergerDef } };
                result.Mergers.Add(merger);
            }

            return result;
        }

        private static ScanPattern CreateScanPattern(CilScanRule scanRule)
        {
            return scanRule.ScanPattern;
#if false
            var    bootstrap        = scanRule as IHasBootstrapPattern;
            string bootstrapPattern = bootstrap == null ? null : bootstrap.BootstrapPattern;

            ScanPattern result;

            var asSingleToken = scanRule as ICilSingleTokenScanRule;
            if (asSingleToken != null && asSingleToken.LiteralText != null)
            {
                result = ScanPattern.CreateLiteral(asSingleToken.LiteralText);
            }
            else
            {
                result = ScanPattern.CreateRegular(scanRule.Pattern, bootstrapPattern);
            }

            return result;
#endif
        }

        private static ScanCondition ConditionFromType(EbnfGrammar result, CilScanCondition scanDef)
        {
            return ConditionFromType(result, scanDef.ScanModeType);
        }

        private static ScanCondition ConditionFromType(EbnfGrammar grammar, Type type)
        {
            if (type == null)
            {
                return null;
            }

            foreach (var cond in grammar.ScanConditions)
            {
                var binding = cond.Joint.The<CilScanConditionDef>();
                if (binding.ConditionType == type)
                {
                    return cond;
                }
            }

            var condition = new ScanCondition(type.FullName);
            condition.Joint.Add(new CilScanConditionDef(type));
            grammar.ScanConditions.Add(condition);
            return condition;
        }

        private static SymbolBase GetResultSymbol(
            EbnfGrammar                 grammar,
            ITokenRefResolver           tokenResolver,
            CilSymbolRef                mainTokenRef,
            IEnumerable<CilSymbolRef>   tokenRefGroups)
        {
            Symbol main = tokenResolver.GetSymbol(mainTokenRef);
            Symbol[] other = (from g in tokenRefGroups
                             select tokenResolver.GetSymbol(g))
                             .ToArray();

            if (other.Length == 0)
            {
                return null;
            }

            if (other.Length == 1)
            {
                return main;
            }

            return new AmbiguousSymbol(main, other);
        }

        private static List<ProductionContextLink> CollectLocalContexts(
            EbnfGrammar             grammar,
            ILrDfa                  lrDfa,
            IList<CilProductionDef> allParseRules)
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
