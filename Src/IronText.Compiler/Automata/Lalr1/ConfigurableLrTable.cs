using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Framework;

namespace IronText.Automata.Lalr1
{
    class ConfigurableLrTable : ILrParserTable
    {
        private readonly IMutableTable<int> data;
        private readonly BnfGrammar         grammar;
        private readonly ILrParserTable     underlyingTable;

        public ConfigurableLrTable(ILrDfa dfa, LanguageFlags flags)
        {
            this.grammar = dfa.Grammar;

            int allTokenCount = grammar.TokenCount + grammar.AmbTokenCount;
            this.data = new MutableTable<int>(dfa.States.Length, allTokenCount);

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

        private bool Configure(ILrDfa dfa, LanguageFlags flags, out ILrParserTable outputTable)
        {
            bool result;

            ComplyWithConfiguration = true;
            switch (flags & LanguageFlags.ParserAlgorithmMask)
            {
                case LanguageFlags.ForceDeterministic:
                    outputTable = BuildCanonicalLRTable(dfa);
                    RequiresGlr = false;
                    result = outputTable != null && !outputTable.RequiresGlr;
                    if (!result)
                    {
                        ComplyWithConfiguration = false;
                        RequiresGlr   = true;
                        outputTable = BuildReductionModifiedLRTable(dfa);
                    }

                    break;
                case LanguageFlags.ForceNonDeterministic:
                    RequiresGlr   = true;
                    outputTable = BuildReductionModifiedLRTable(dfa);
                    result = outputTable != null;
                    break;
                case LanguageFlags.AllowNonDeterministic:
                    outputTable = BuildCanonicalLRTable(dfa);
                    result = outputTable != null && !outputTable.RequiresGlr;
                    if (!result)
                    {
                        data.Clear();
                        goto case LanguageFlags.ForceNonDeterministic;
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

                foreach (var ambToken in grammar.AmbTokens)
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
                            data.Set(i, ambToken.Id, 0);
                            break;
                        case 1:
                            {
                                var pair = validTokenActions.First();
                                if (pair.Key == ambToken.MainToken)
                                {
                                    // ambToken action is the same as for the main token
                                    data.Set(i, ambToken.Id, pair.Value);
                                }
                                else
                                {
                                    // Resolve ambToken to a one of the underlying tokens.
                                    // In runtime transition will be acceptable when this token
                                    // is in Msg and non-acceptable when this particular token
                                    // is not in Msg.
                                    var action = new ParserAction { Kind = ParserActionKind.Resolve, Value1 = pair.Key };
                                    data.Set(i, ambToken.Id, ParserAction.Encode(action));
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
                                data.Set(i, ambToken.Id, ParserAction.Encode(forkAction));
                            }

                            break;
                    }
                }
            }

            return true;
        }
    }
}
