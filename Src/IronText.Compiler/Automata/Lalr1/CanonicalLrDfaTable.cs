using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Reflection.Reporting;
using IronText.Runtime;
using IronText.Collections;
using System;

namespace IronText.Automata.Lalr1
{
    class CanonicalLrDfaTable : ILrParserTable
    {
        private readonly GrammarAnalysis grammar;
        private readonly ParserConflictResolver conflictResolver;
        private readonly Dictionary<TransitionKey, ParserConflictInfo> transitionToConflict 
            = new Dictionary<TransitionKey, ParserConflictInfo>();
        private readonly IMutableTable<ParserDecision> actionTable;

        public CanonicalLrDfaTable(
            ILrDfa          dfa,
            GrammarAnalysis grammar,
            ParserConflictResolver conflictResolver)
        {
            this.grammar = grammar;
            this.conflictResolver = conflictResolver;
            this.actionTable = new MutableTable<ParserDecision>(
                                dfa.States.Length,
                                grammar.TotalSymbolCount);

            FillDfaTable(dfa.States);
            HasUnresolvedTerminalAmbiguities = FillAmbiguousTerminalActions(dfa.States);
            Conflicts = FillConflictActions();
        }

        public ParserConflictInfo[] Conflicts { get; }

        public bool HasUnresolvedTerminalAmbiguities { get;  }

        public ITable<ParserDecision> ParserActionTable => actionTable;

        private ParserConflictInfo[] FillConflictActions()
        {
            var result = new List<ParserConflictInfo>();

            for (int state = 0; state != actionTable.RowCount; ++state)
            for (int token = 0; token != actionTable.ColumnCount; ++token)
                {
                    var decision = actionTable.Get(state, token);
                    if (decision == ParserDecision.NoAlternatives || decision.IsDeterminisic)
                    {
                        continue;
                    }

                    var conflict = ToConflict(state, token, decision);
                    int conflictIndex = result.Count;

                    result.Add(conflict);
                }

            return result.ToArray();
        }

        private static ParserConflictInfo ToConflict(int row, int column, ParserDecision decision)
        {
            var result = new ParserConflictInfo(row, column);

            foreach (var alternative in decision.Alternatives())
            {
                if (decision.Instructions.Count != 1)
                {
                    throw new InvalidOperationException(
                        "Internal error: reafctoring of parser actions towards bytecode.");
                }

                result.AddAction(decision.Instructions[0]);
            }

            return result;
        }

        private void FillDfaTable(DotState[] states)
        {
            foreach (var state in states)
            {
                foreach (var item in state.Items)
                {
                    if (!item.IsReduce)
                    {
                        AssignAction(
                            state.Index,
                            item.NextToken, 
                            ParserInstruction.Shift(
                                state.GetNextIndex(item.NextToken)));
                    }
                    else if (item.IsAugmented)
                    {
                        AssignAction(
                            state.Index,
                            PredefinedTokens.Eoi,
                            ParserInstruction.AcceptAction);
                    }
                    else
                    {
                        var action = ParserInstruction.Reduce(item.ProductionId);

                        foreach (var lookahead in item.LA)
                        {
                            AssignAction(
                                state.Index,
                                lookahead,
                                action);
                        }
                    }
                }
            }
        }

        private void AssignAction(int state, int token, ParserInstruction action)
        {
            AssignAction(state, token, new ParserDecision(action));
        }

        private void AssignAction(int state, int token, ParserDecision decision)
        {
            ParserDecision current = actionTable.Get(state, token);
            ParserDecision resolved;

            if (current == ParserDecision.NoAlternatives)
            {
                resolved = decision;
            }
            else if (current.Equals(decision))
            {
                resolved = current;
            }
            else if (
                current.Instructions.Count == 1
                && conflictResolver.TryResolve(current, decision, token, out resolved))
            {
            }
            else
            {
                resolved = current.Alternate(decision);
            }

            actionTable.Set(state, token, resolved);
        }

        private bool FillAmbiguousTerminalActions(DotState[] states)
        {
            bool result = false;

            for (int i = 0; i != states.Length; ++i)
            {
                var state = states[i];

                foreach (var ambiguousTerm in grammar.AmbiguousSymbols)
                {
                    var tokenToDecision = new Dictionary<int,ParserDecision>();
                    foreach (int token in ambiguousTerm.Alternatives)
                    {
                        var decision = actionTable.Get(i, token);
                        if (decision != ParserDecision.NoAlternatives)
                        {
                            tokenToDecision.Add(token, decision);
                        }
                    }

                    switch (tokenToDecision.Values.Distinct().Count())
                    {
                        case 0:
                            // AmbToken is entirely non-acceptable for this state
                            actionTable.Set(
                                i,
                                ambiguousTerm.EnvelopeIndex,
                                ParserDecision.NoAlternatives);
                            break;
                        case 1:
                            {
                                var pair = tokenToDecision.First();
                                if (pair.Key == ambiguousTerm.MainToken)
                                {
                                    // ambiguousToken action is the same as for the main token
                                    actionTable.Set(
                                        i,
                                        ambiguousTerm.EnvelopeIndex,
                                        pair.Value);
                                }
                                else
                                {
                                    // Resolve ambToken to a one of the underlying tokens.
                                    // In runtime transition will be acceptable when this token
                                    // is in Msg and non-acceptable when this particular token
                                    // is not in Msg.
                                    var action = ParserInstruction.Resolve(pair.Key);
                                    actionTable.Set(
                                        i,
                                        ambiguousTerm.EnvelopeIndex,
                                        new ParserDecision(action));
                                }
                            }

                            break;
                        default:
                            // GLR parser is required to handle terminal token alternatives.
                            result = true;

                            // No needed for GLR but but for the sake of explicitness
                            actionTable.Set(
                                i,
                                ambiguousTerm.EnvelopeIndex,
                                ParserDecision.NoAlternatives);
                            break;
                    }
                }
            }

            return result;
        }
    }
}
