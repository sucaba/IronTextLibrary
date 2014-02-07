using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Reflection;
using IronText.Reporting;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class ConfigurableLrTable : ILrParserTable
    {
        private readonly IMutableTable<int>   data;
        private readonly GrammarAnalysis   grammar;
        private readonly ILrParserTable       underlyingTable;

        public ConfigurableLrTable(ILrDfa dfa, RuntimeOptions flags)
        {
            this.grammar = dfa.GrammarAnalysis;

            this.data = new MutableTable<int>(dfa.States.Length, grammar.Symbols.Count);

            Configure(dfa, flags, out underlyingTable); 
        }

        public bool ComplyWithConfiguration { get; private set; }

        public bool RequiresGlr { get; private set; }

        public ITable<int> GetParserActionTable() { return data; }

        public int[] GetConflictActionTable()
        {
            return underlyingTable.GetConflictActionTable();
        }

        public ParserConflictInfo[] Conflicts { get { return underlyingTable.Conflicts; } }

        private bool Configure(ILrDfa dfa, RuntimeOptions flags, out ILrParserTable outputTable)
        {
            bool result;

            ComplyWithConfiguration = true;
            switch (flags & RuntimeOptions.ParserAlgorithmMask)
            {
                case RuntimeOptions.ForceDeterministic:
                    outputTable = BuildCanonicalLRTable(dfa);
                    RequiresGlr = false;
                    result = outputTable != null && !outputTable.RequiresGlr;
                    if (!result)
                    {
                        data.Clear();
                        ComplyWithConfiguration = false;
                        RequiresGlr   = true;
                        outputTable = BuildReductionModifiedLRTable(dfa);
                    }

                    break;
                case RuntimeOptions.ForceNonDeterministic:
                    RequiresGlr   = true;
                    outputTable = BuildReductionModifiedLRTable(dfa);
                    result = outputTable != null;
                    break;
                case RuntimeOptions.AllowNonDeterministic:
                    outputTable = BuildCanonicalLRTable(dfa);
                    result = outputTable != null && !outputTable.RequiresGlr;
                    if (!result)
                    {
                        data.Clear();
                        goto case RuntimeOptions.ForceNonDeterministic;
                    }

                    RequiresGlr   = false;
                    break;
                default:
#if DEBUG
                    throw new InvalidOperationException(
                        "Internal error: unsupported language flags: " + (int)flags);
#else
                    result = false;
                    outputTable = null;
                    break;
#endif
            }

            return result;
        }

        private ILrParserTable BuildReductionModifiedLRTable(ILrDfa dfa)
        {
            ILrParserTable result = new ReductionModifiedLrDfaTable(dfa, this.data);
            FillAmbiguousTokenActions(dfa.States, isGlr: true);
            return result;
        }

        private ILrParserTable BuildCanonicalLRTable(ILrDfa dfa)
        {
            ILrParserTable result = new CanonicalLrDfaTable(dfa, this.data);
            if (result.RequiresGlr || !FillAmbiguousTokenActions(dfa.States, isGlr:false))
            {
                result = null;
            }

            return result;
        }

        private bool FillAmbiguousTokenActions(DotState[] states, bool isGlr)
        {
            for (int i = 0; i != states.Length; ++i)
            {
                var state = states[i];

                foreach (var ambToken in grammar.AmbiguousSymbols)
                {
                    var validTokenActions = new Dictionary<int,int>();
                    foreach (int token in ambToken.Tokens)
                    {
                        int cell = data.Get(i, token);
                        if (cell == 0)
                        {
                            continue;
                        }

                        validTokenActions.Add(token, cell);
                    }

                    switch (validTokenActions.Count)
                    {
                        case 0:
                            // AmbToken is entirely non-acceptable for this state
                            data.Set(i, ambToken.Index, 0);
                            break;
                        case 1:
                            {
                                var pair = validTokenActions.First();
                                if (pair.Key == ambToken.MainToken)
                                {
                                    // ambToken action is the same as for the main token
                                    data.Set(i, ambToken.Index, pair.Value);
                                }
                                else
                                {
                                    // Resolve ambToken to a one of the underlying tokens.
                                    // In runtime transition will be acceptable when this token
                                    // is in Msg and non-acceptable when this particular token
                                    // is not in Msg.
                                    var action = new ParserAction { Kind = ParserActionKind.Resolve, Value1 = pair.Key };
                                    data.Set(i, ambToken.Index, ParserAction.Encode(action));
                                }
                            }

                            break;
                        default:
                            if (validTokenActions.Values.Distinct().Count() == 1)
                            {
                                // Multiple tokens but with the same action
                                goto case 1;
                            }

                            if (!isGlr)
                            {
                                return false;
                            }

                            // This kind of ambiguity requires GLR to follow all alternate tokens
                            {
                                var pair = validTokenActions.First();
                                var forkAction = new ParserAction { Kind = ParserActionKind.Fork, Value1 = pair.Key };
                                data.Set(i, ambToken.Index, ParserAction.Encode(forkAction));
                            }

                            break;
                    }
                }
            }

            return true;
        }
    }
}
