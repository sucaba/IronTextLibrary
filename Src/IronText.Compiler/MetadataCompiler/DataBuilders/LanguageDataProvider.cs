using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Build;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Logging;
using IronText.Misc;

namespace IronText.MetadataCompiler
{
    class LanguageDataProvider : ResourceGetter<LanguageData>
    {
        private readonly LanguageName languageName;
        private readonly bool         bootstrap;

        public LanguageDataProvider(LanguageName name, bool bootstrap)
        {
            this.languageName = name;
            this.bootstrap = bootstrap;
            Getter = Build;
        }

        private bool Build(ILogging logging, out LanguageData result)
        {
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Verbose,
                    Member = languageName.DefinitionType,
                    Message = string.Format("Started LanguageData build for {0}", languageName.FullName)
                });

            var definition = new LanguageDefinition(languageName.DefinitionType, logging);
            var tokenResolver = definition.TokenRefResolver;

            List<List<GrammarActionBuilder>> ruleActionBuilders;
            var grammar = BuildGrammar(definition, out ruleActionBuilders);

            foreach (SwitchRule switchRule in definition.SwitchRules)
            {
                if (null == tokenResolver.Resolve(switchRule.Tid))
                {
                    throw new InvalidOperationException("No token identity");
                }
            }

            // Build parsing tables
            ILrDfa parserDfa = new Lalr1Dfa(grammar, LrTableOptimizations.Default);

            ILrParserTable lrTable = new CanonicalLrDfaTable(parserDfa);
            bool isAmbiguous 
                =  Attributes.First<LanguageAttribute>(languageName.DefinitionType).ForceGlr
                || lrTable.GetConflictActionTable().Length > 0;

            ILrParserTable parserTable;
#if !ELKHOUND
            if (isAmbiguous)
            {
                parserDfa = new Lalr1Dfa(grammar, LrTableOptimizations.None);
                lrTable = new CanonicalLrDfaTable(parserDfa);
                parserTable = new ReductionModifiedLrDfaTable(parserDfa);
            }
            else
#endif
            {
                parserTable = lrTable;
            }

            var localParseContexts = CollectLocalContexts(grammar, parserDfa, definition.ParseRules);

            // Prepare language data for language assembly generation
            result = new LanguageData
            {
                LanguageName        = languageName,
                IsAmbiguous         = isAmbiguous,
                RootContextType     = languageName.DefinitionType,
                Grammar             = grammar,
                ParserStates        = parserDfa.States,
                StateToSymbolTable  = parserDfa.GetStateToSymbolTable(),
                ParserActionTable   = parserTable.GetParserActionTable(),
                ParserConflictActionTable = parserTable.GetConflictActionTable(),

                Lalr1ParserActionTable = lrTable.GetParserActionTable(),
                Lalr1ParserConflictActionTable = lrTable.GetConflictActionTable(),

                TokenRefResolver    = tokenResolver,

                LocalParseContexts  = localParseContexts.ToArray(),
                RuleActionBuilders  = ruleActionBuilders.Select(bs => bs == null ? null : bs.ToArray()).ToArray(),
                MergeRules          = definition.MergeRules.ToArray(),
                SwitchRules         = definition.SwitchRules.ToArray(),
                ScanModes           = definition.ScanModes.ToArray(),
            };

            if (!bootstrap)
            {
                result.ScanModeTypeToDfa = new Dictionary<Type, ITdfaData>();
                foreach (ScanMode scanMode in result.ScanModes)
                {
                    var descr = ScannerDescriptor.FromScanRules(
                                                scanMode.ScanModeType.FullName,
                                                scanMode.ScanRules,
                                                logging);
                    ITdfaData data;
#if false
                    var regTree2 = new RegularTree(descr.MakeAst());
                    data = new RegularToDfaAlgorithm(regTree2).Data;
#else
                    var literalToAction = new Dictionary<string,int>();
                    var ast = descr.MakeAst(literalToAction);
                    if (ast == null)
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

                    var regTree2 = new RegularTree(ast);
                    data = new RegularToTdfaAlgorithm(regTree2, literalToAction).Data;
#endif

                    result.ScanModeTypeToDfa[scanMode.ScanModeType] = data;
                }

                foreach (var dataAction in definition.LanguageDataActions)
                {
                    dataAction(result);
                }
            }

            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Verbose,
                    Member = languageName.DefinitionType,
                    Message = string.Format("Done LanguageData build for {0}", languageName.FullName)
                });

            return true;
        }

        private static BnfGrammar BuildGrammar(
            LanguageDefinition definition,
            out List<List<GrammarActionBuilder>> ruleActionBuilders)
        {
            var grammar = new BnfGrammar();

            // Define grammar tokens
            var tokenResolver = definition.TokenRefResolver;

            foreach (var def in tokenResolver.Definitions)
            {
                if (def.TokenType == typeof(Exception))
                {
                    def.Id = BnfGrammar.Error;
                }
                else
                {
                    def.Id = grammar.DefineToken(def.Name, def.Categories);
                }
            }

            grammar.StartToken = tokenResolver.GetId(definition.Start);

            ruleActionBuilders = new List<List<GrammarActionBuilder>> { null }; // first null is for the augumented start rule

            // Define grammar rules
            foreach (var rule in definition.ParseRules)
            {
                int leftId = tokenResolver.GetId(rule.Left);
                int[] parts = Array.ConvertAll(rule.Parts, tokenResolver.GetId);

                // Try to find existing rules whith same token-signature
                int ruleId = grammar.FindRuleId(leftId, parts);
                if (ruleId < 0)
                {
                    ruleId = grammar.DefineRule(leftId, parts);
                    ruleActionBuilders.Add(new List<GrammarActionBuilder>());
                }

                if (rule.Precedence != null)
                {
                    var existingPrecedence = grammar.Rules[ruleId].Precedence;
                    if (existingPrecedence != null)
                    {
                        if (!object.Equals(rule.Precedence, existingPrecedence))
                        {
                            throw new InvalidOperationException(
                                "Two rule definitions of the same rule have conflicting precedence: " +
                                rule);
                        }
                    }
                    else
                    {
                        grammar.SetRulePrecedence(ruleId, rule.Precedence);
                    }
                }

                // Each rule may have multiple action builders
                ruleActionBuilders[ruleId].Add(rule.ActionBuilder);

                rule.Index = ruleId;
            }

            foreach (var mergeRule in definition.MergeRules)
            {
                mergeRule.TokenId = tokenResolver.GetId(mergeRule.Token);
            }

            foreach (KeyValuePair<TokenRef, Precedence> pair in definition.Precedence)
            {
                int id = tokenResolver.GetId(pair.Key);
                grammar.SetTermPrecedence(id, pair.Value);
            }

            grammar.Freeze();

            return grammar;
        }

        private static List<LocalParseContext> CollectLocalContexts(
            BnfGrammar       grammar,
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
                    if (item.Pos == 0 || item.Pos == item.Rule.Parts.Length)
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
            return "data for " + languageName.DefinitionType.FullName + " language definition";
        }
    }
}
