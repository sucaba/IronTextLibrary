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
using IronText.Extensibility.Bindings.Cil;
using IronText.Compiler;
using IronText.Compiler.Analysis;
using IronText.Analysis;

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

            WithTimeLogging(
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
            WithTimeLogging(
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

            var localParseContexts = CollectLocalContexts(grammar, parserDfa, definition.ParseRules);

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
            result.MergeRules          = definition.MergeRules.ToArray();

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
                var conditionBinding = condition.Joint.The<CilScanConditionBinding>();

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

            result.Start = tokenResolver.GetSymbol(definition.Start);

            // Define grammar rules
            foreach (var ruleDef in definition.ParseRules)
            {
                Symbol outcome = tokenResolver.GetSymbol(ruleDef.Left);
                var pattern = Array.ConvertAll(ruleDef.Parts, tokenResolver.GetSymbol);

                // Try to find existing rules whith same token-signature
                Production production;
                if (result.Productions.FindOrAdd(outcome, pattern, out production))
                {
                    production.Action =
                        new SimpleProductionAction(pattern.Length)
                        {
                            Joint = { new CilProductionActionBinding(ruleDef.ActionBuilder) }
                        };
                }
                else
                {
                    // Note: Following adds action alternative as another
                    // binding such that stack reduction happens after all
                    // bindings are executed.  It would be mistake to do
                    // Actions.Add(...) in this case because semantical 
                    // reduction happens after each ProductionAction.
                    var action = (SimpleProductionAction)production.Action;
                    action.Joint.Add(new CilProductionActionBinding(ruleDef.ActionBuilder));
                }

                if (!AssignPrecedence(production, ruleDef.Precedence))
                {
                    throw new InvalidOperationException(
                        "Two or more production definitions have conflicting precedence: " +
                        ruleDef);
                }

                ruleDef.Index = production.Index;
            }

            foreach (KeyValuePair<TokenRef, Precedence> pair in definition.Precedence)
            {
                int id = tokenResolver.GetId(pair.Key);
                result.Symbols[id].Precedence = pair.Value;
            }

            foreach (var scanMode in definition.ScanModes)
            {
                var condition = ConditionFromType(result, scanMode.ScanModeType);
                

                foreach (var scanRule in scanMode.ScanRules)
                {
                    int mainToken = tokenResolver.GetId(scanRule.MainTokenRef);
                    int[] tokens = scanRule
                            .GetTokenRefGroups()
                            .Select(trs => tokenResolver.GetId(trs[0]))
                            .ToArray();

                    SymbolBase outcome = GetResultSymbol(result, tokenResolver, scanRule.MainTokenRef, scanRule.GetTokenRefGroups());

                    var asSingleToken = scanRule as ISingleTokenScanRule;
                    ScanPattern pattern;
                    if (asSingleToken != null && asSingleToken.LiteralText != null)
                    {
                        pattern = ScanPattern.CreateLiteral(asSingleToken.LiteralText);
                    }
                    else if (scanRule is IBootstrapScanRule) 
                    {
                        pattern = ScanPattern.CreateRegular(
                                    scanRule.Pattern,
                                    ((IBootstrapScanRule)scanRule).BootstrapRegexPattern);
                    }
                    else
                    {
                        pattern = ScanPattern.CreateRegular(scanRule.Pattern);
                    }

                    var scanProduction = new ScanProduction(
                        pattern,
                        outcome,
                        nextCondition: ConditionFromType(result, scanRule.NextModeType),
                        disambiguation: scanRule.Disambiguation);
                    scanProduction.Joint.Add(
                        new CilScanProductionBinding(scanRule.DefiningMember, scanRule.ActionBuilder));

                    condition.ScanProductions.Add(scanProduction);
                }
            }

            foreach (var mergeRule in definition.MergeRules)
            {
                int token = tokenResolver.GetId(mergeRule.Token);
                mergeRule.TokenId = token;

                var merger = new Merger((Symbol)result.Symbols[token]);
                merger.Joint.Add(new CilMergerBinding { Builder = mergeRule.ActionBuilder });
                
                result.Mergers.Add(merger);
            }

            return result;
        }

        private static ScanCondition ConditionFromType(EbnfGrammar result, Type type)
        {
            if (type == null)
            {
                return null;
            }

            foreach (var cond in result.ScanConditions)
            {
                var binding = cond.Joint.The<CilScanConditionBinding>();
                if (binding.ConditionType == type)
                {
                    return cond;
                }
            }

            var condition = new ScanCondition(type.FullName);
            condition.Joint.Add(new CilScanConditionBinding(type));
            result.ScanConditions.Add(condition);
            return condition;
        }

        private static SymbolBase GetResultSymbol(
            EbnfGrammar             grammar,
            ITokenRefResolver       tokenResolver,
            TokenRef                mainTokenRef,
            IEnumerable<TokenRef[]> tokenRefGroups)
        {
            Symbol main = tokenResolver.GetSymbol(mainTokenRef);
            Symbol[] other = (from g in tokenRefGroups
                             select tokenResolver.GetSymbol(g[0]))
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

        private static List<LocalParseContext> CollectLocalContexts(
            EbnfGrammar      grammar,
            ILrDfa           lrDfa,
            IList<ParseRule> allParseRules)
        {
            var result = new List<LocalParseContext>();
            var states = lrDfa.States;
            int stateCount = states.Length;
            for (int parentState = 0; parentState != stateCount; ++parentState)
            {
                var stateItems = states[parentState];

                foreach (var item in stateItems.Items)
                {
                    if (item.Position == 0 || item.IsReduce)
                    {
                        // Skip non-kernel, augmented (start rule) items and end-of-rule states
                        continue;
                    }

                    foreach (var parentRuleDef in allParseRules)
                    {
                        if (parentRuleDef.Index != item.ProductionId || !parentRuleDef.IsContextRule)
                        {
                            continue;
                        }

                        Debug.Assert(!parentRuleDef.Parts[0].IsLiteral);
                        Type contextTokenType = parentRuleDef.Parts[0].TokenType;

                        TokenRef childToken = parentRuleDef.Parts[item.Position];

                        foreach (ParseRule childRule in allParseRules)
                        {
                            if (!childRule.Left.Equals(childToken) 
                                || childRule.InstanceDeclaringType == null)
                            {
                                continue;
                            }

                            var contextBrowser = new ContextBrowser(contextTokenType);
                            if (null == contextBrowser.GetGetterPath(childRule.InstanceDeclaringType))
                            {
                                continue;
                            }

                            var l = new LocalParseContext
                                {
                                    ParentState = parentState,
                                    ContextTokenType = contextTokenType,
                                    ContextLookbackPos = item.Position,
                                    ChildType = childRule.InstanceDeclaringType
                                };

                            if (!result.Contains(l))
                            {
                                result.Add(l);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static bool TypeHasContextPropertyOfType(
            Type parentDeclaringType, 
            Type childRuleDeclaringType)
        {
            var contextBrowser = new ContextBrowser(parentDeclaringType);
            return null != contextBrowser.GetGetterPath(childRuleDeclaringType);
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

        private void WithTimeLogging(Action action, string activityName)
        {
            Verbose("Started {0} for {1}", activityName, languageName.Name);

            try
            {
                action();
            }
            catch (Exception e)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Member   = languageName.DefinitionType,
                        Message  = e.Message
                    });
            }
            finally
            {
                Verbose("Done {0} for {1}", activityName, languageName.Name);
            }
        }

        private void Verbose(string fmt, params object[] args)
        {
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Verbose,
                    Member = languageName.DefinitionType,
                    Message = string.Format(fmt, args)
                });
        }

        private void Error(string fmt, params object[] args)
        {
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Error,
                    Member = languageName.DefinitionType,
                    Message = string.Format(fmt, args)
                });
        }

        private static bool AssignPrecedence(Production prod, Precedence value)
        {
            if (value != null)
            {
                var existingPrecedence = prod.ExplicitPrecedence;
                if (existingPrecedence != null)
                {
                    if (!object.Equals(value, existingPrecedence))
                    {
                        return false;
                    }
                }
                else
                {
                    prod.ExplicitPrecedence = value;
                }
            }

            return true;
        }
    }
}
