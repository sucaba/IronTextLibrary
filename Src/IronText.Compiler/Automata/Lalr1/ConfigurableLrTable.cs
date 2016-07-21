using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class ConfigurableLrTable : ILrParserTable
    {
        private readonly IMutableTable<ParserAction> data;
        private readonly GrammarAnalysis grammar;
        private readonly ILrParserTable  underlyingTable;

        public ConfigurableLrTable(ILrDfa dfa, RuntimeOptions flags)
        {
            this.grammar = dfa.GrammarAnalysis;

            this.data = new MutableTable<ParserAction>(dfa.States.Length, grammar.TotalSymbolCount);

            Configure(dfa, flags, out underlyingTable); 
        }

        public bool ComplyWithConfiguration { get; private set; }

        public ParserRuntime TargetRuntime { get; private set; }

        public ITable<ParserAction> GetParserActionTable() { return data; }

        public ParserAction[] GetConflictActionTable()
        {
            return underlyingTable.GetConflictActionTable();
        }

        public ParserConflictInfo[] Conflicts { get { return underlyingTable.Conflicts; } }

        private void Configure(ILrDfa dfa, RuntimeOptions flags, out ILrParserTable outputTable)
        {
            ComplyWithConfiguration = true;
            switch (flags & RuntimeOptions.ParserAlgorithmMask)
            {
                case RuntimeOptions.ForceGeneric:
                    TargetRuntime = ParserRuntime.Generic;
                    outputTable = BuildLRTable(dfa, forceDeterministic: false);
                    break;
                case RuntimeOptions.ForceDeterministic:
                    outputTable = BuildGlrTable(dfa);
                    ComplyWithConfiguration = TargetRuntime == ParserRuntime.Deterministic;
                    break;
                case RuntimeOptions.ForceNonDeterministic:
                    TargetRuntime = ParserRuntime.Glr;
                    outputTable = BuildLRTable(dfa, forceDeterministic: false);
                    break;
                case RuntimeOptions.AllowNonDeterministic:
                    outputTable = BuildGlrTable(dfa);
                    break;
                default:
                    throw new InvalidOperationException(
                        "Internal error: unsupported language flags: " + (int)flags);
            }
        }

        private ILrParserTable BuildGlrTable(ILrDfa dfa)
        {
            ILrParserTable outputTable = BuildLRTable(dfa, forceDeterministic: true);
            if (outputTable == null || outputTable.TargetRuntime != ParserRuntime.Deterministic)
            {
                data.Clear();
                TargetRuntime = ParserRuntime.Glr;
                outputTable = BuildLRTable(dfa, forceDeterministic: false);
            }
            else
            {
                TargetRuntime = ParserRuntime.Deterministic;
            }

            return outputTable;
        }

        private ILrParserTable BuildGlrLRTable(ILrDfa dfa)
        {
            return BuildLRTable(dfa, forceDeterministic: false);
        }

        private ILrParserTable BuildLRTable(ILrDfa dfa, bool forceDeterministic)
        {
            ILrParserTable result = new CanonicalLrDfaTable(dfa, this.data, forceDeterministic);
            if ((forceDeterministic && result.TargetRuntime != ParserRuntime.Deterministic)
                || !FillAmbiguousTokenActions(dfa.States, forceDeterministic))
            {
                result = null;
            }

            return result;
        }

        private bool FillAmbiguousTokenActions(DotState[] states, bool forceDeterministic)
        {
            for (int i = 0; i != states.Length; ++i)
            {
                var state = states[i];

                foreach (var ambToken in grammar.AmbiguousSymbols)
                {
                    var validTokenActions = new Dictionary<int,ParserAction>();
                    foreach (int token in ambToken.Alternatives)
                    {
                        var action = data.Get(i, token);
                        if (action == default(ParserAction))
                        {
                            continue;
                        }

                        validTokenActions.Add(token, action);
                    }

                    switch (validTokenActions.Count)
                    {
                        case 0:
                            // AmbToken is entirely non-acceptable for this state
                            data.Set(i, ambToken.EnvelopeIndex, ParserAction.FailAction);
                            break;
                        case 1:
                            {
                                var pair = validTokenActions.First();
                                if (pair.Key == ambToken.MainToken)
                                {
                                    // ambToken action is the same as for the main token
                                    data.Set(i, ambToken.EnvelopeIndex, pair.Value);
                                }
                                else
                                {
                                    // Resolve ambToken to a one of the underlying tokens.
                                    // In runtime transition will be acceptable when this token
                                    // is in Msg and non-acceptable when this particular token
                                    // is not in Msg.
                                    var action = new ParserAction { Kind = ParserActionKind.Resolve, Value1 = pair.Key };
                                    data.Set(i, ambToken.EnvelopeIndex, action);
                                }
                            }

                            break;
                        default:
                            if (validTokenActions.Values.Distinct().Count() == 1)
                            {
                                // Multiple tokens but with the same action
                                goto case 1;
                            }

                            if (forceDeterministic)
                            {
                                return false;
                            }

                            // This kind of ambiguity requires GLR to follow all alternate tokens
                            {
                                var pair = validTokenActions.First();
                                var forkAction = new ParserAction
                                {
                                    Kind   = ParserActionKind.Fork,
                                    Value1 = pair.Key
                                };
                                data.Set(i, ambToken.EnvelopeIndex, forkAction);
                            }

                            break;
                    }
                }
            }

            return true;
        }
    }
}
