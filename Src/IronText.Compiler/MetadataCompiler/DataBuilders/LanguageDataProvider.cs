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

            List<List<ProductionActionBuilder>> ruleActionBuilders;
            var grammar = BuildGrammar(definition, out ruleActionBuilders);
            var grammarAnalysis = new EbnfGrammarAnalysis(grammar);

            foreach (SwitchRule switchRule in definition.SwitchRules)
            {
                if (null == tokenResolver.Resolve(switchRule.Tid))
                {
                    throw new InvalidOperationException("No token identity");
                }
            }

            if (!bootstrap)
            {
                if (!BuildScanner(definition, grammarAnalysis, tokenResolver, result))
                {
                    return false;
                }
            }

            // Build parsing tables
            ILrDfa parserDfa = null;

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
            result.RuleActionBuilders  = ruleActionBuilders.Select(bs => bs == null ? null : bs.ToArray()).ToArray();
            result.MergeRules          = definition.MergeRules.ToArray();
            result.SwitchRules         = definition.SwitchRules.ToArray();
            result.ScanModes           = definition.ScanModes.ToArray();

            if (!bootstrap)
            {
                foreach (var reportBuilder in reportBuilders)
                {
                    reportBuilder(result);
                }
            }

            return true;
        }

        private bool BuildScanner(
            LanguageDefinition  definition,
            EbnfGrammarAnalysis grammar,
            ITokenRefResolver   tokenResolver,
            LanguageData        result)
        {
            var scanModeTypeToDfa = new Dictionary<Type, ITdfaData>();

            int actionCount = definition.ScanModes.Sum(m => m.ScanRules.Count);
            IScanAmbiguityResolver scanAmbiguityResolver
                                    = new ScanAmbiguityResolver(
                                            grammar.TokenSet,
                                            actionCount);

            int firstScanAction = 0;

            foreach (ScanMode mode in definition.ScanModes)
            {
                int count = mode.ScanRules.Count;

                ITdfaData tdfaData;
                if (!BuildScanModeDfa(logging, mode, out tdfaData))
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

                    return false;
                }

                scanModeTypeToDfa[mode.ScanModeType] = tdfaData;

                // For each action store information about produced tokens
                foreach (var scanRule in mode.scanRules)
                {
                    scanAmbiguityResolver.RegisterAction(
                        scanRule.Index,
                        scanRule.Disambiguation,
                        tokenResolver.GetId(scanRule.MainTokenRef),
                        scanRule
                            .GetTokenRefGroups()
                            .Select(trs => trs[0])
                            .Select(tokenResolver.GetId));
                }

                // For each 'ambiguous scanner state' deduce all tokens
                // which can be produced in this state.
                foreach (var state in tdfaData.EnumerateStates())
                {
                    scanAmbiguityResolver.RegisterState(state);
                }

                firstScanAction += count;
            }

            scanAmbiguityResolver.DefineAmbiguities(grammar);

            result.ScanModeTypeToDfa = scanModeTypeToDfa;

            return true;
        }

        private static bool BuildScanModeDfa(ILogging logging, ScanMode scanMode, out ITdfaData tdfaData)
        {
            var descr = ScannerDescriptor.FromScanRules(
                                        scanMode.ScanModeType.FullName,
                                        scanMode.ScanRules,
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

        private static EbnfGrammar BuildGrammar(
            LanguageDefinition definition,
            out List<List<ProductionActionBuilder>> ruleActionBuilders)
        {
            var grammar = new EbnfGrammar();

            // Define grammar tokens
            var tokenResolver = definition.TokenRefResolver;

            foreach (var def in tokenResolver.Definitions)
            {
                if (def.TokenType == typeof(Exception))
                {
                    def.Id = EbnfGrammar.Error;
                }
                else
                {
                    var symbol = new Symbol(def.Name) { Categories = def.Categories };
                    grammar.Symbols.Add(symbol);
                    def.Id = symbol.Index;
                }
            }

            grammar.StartToken = tokenResolver.GetId(definition.Start);

            ruleActionBuilders = new List<List<ProductionActionBuilder>> { null }; // first null is for the augumented start rule

            // Define grammar rules
            foreach (var ruleDef in definition.ParseRules)
            {
                int outcome = tokenResolver.GetId(ruleDef.Left);
                int[] pattern = Array.ConvertAll(ruleDef.Parts, tokenResolver.GetId);

                // Try to find existing rules whith same token-signature
                Production production;
                if (grammar.Productions.FindOrAdd(outcome, pattern, out production))
                {
                    ruleActionBuilders.Add(new List<ProductionActionBuilder>());
                    production.Actions.Add(
                        new ProductionAction
                        {
                            Bindings = {
                                new CilProductionActionBinding(ruleDef.ActionBuilder)
                            }
                        });
                }
                else
                {
                    // Note: Following adds action alternative as another
                    // binding such that stack reduction happens after all
                    // bindings are executed.  It would be mistake to do
                    // Actions.Add(...) in this case because reduction happens
                    // after each ProductionAction.
                    production.Actions.First.Bindings.Add(
                        new CilProductionActionBinding(ruleDef.ActionBuilder));
                }

                // Each rule may have multiple action builders
                ruleActionBuilders[production.Index].Add(ruleDef.ActionBuilder);

                if (!AssignPrecedence(production, ruleDef.Precedence))
                {
                    throw new InvalidOperationException(
                        "Two or more production definitions have conflicting precedence: " +
                        ruleDef);
                }

                ruleDef.Index = production.Index;
            }

            foreach (var mergeRule in definition.MergeRules)
            {
                mergeRule.TokenId = tokenResolver.GetId(mergeRule.Token);
            }

            foreach (KeyValuePair<TokenRef, Precedence> pair in definition.Precedence)
            {
                int id = tokenResolver.GetId(pair.Key);
                grammar.Symbols[id].Precedence = pair.Value;
            }

            // grammar.Freeze();

            return grammar;
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
                    if (item.Pos == 0 || item.Pos == item.Rule.Pattern.Length)
                    {
                        // Skip non-kernel, augmented (start rule) items and end-of-rule states
                        continue;
                    }

                    foreach (var parentRuleDef in allParseRules)
                    {
                        if (parentRuleDef.Index != item.RuleId || !parentRuleDef.IsContextRule)
                        {
                            continue;
                        }

                        Debug.Assert(!parentRuleDef.Parts[0].IsLiteral);
                        Type contextTokenType = parentRuleDef.Parts[0].TokenType;

                        TokenRef childToken = parentRuleDef.Parts[item.Pos];

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
                                    ContextLookbackPos = item.Pos,
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
