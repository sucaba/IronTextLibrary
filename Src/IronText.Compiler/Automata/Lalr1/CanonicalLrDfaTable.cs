using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;
using System;

namespace IronText.Automata.Lalr1
{
    class CanonicalLrDfaTable : ILrParserTable
    {
        private readonly GrammarAnalysis grammar;
        private readonly ParserConflictResolver conflictResolver;
        private readonly Dictionary<TransitionKey, ParserConflictInfo> transitionToConflict 
            = new Dictionary<TransitionKey, ParserConflictInfo>();
        private readonly IMutableTable<ParserInstruction> actionTable;
        private bool hasTerminalAmbiguities;

        public CanonicalLrDfaTable(
            ILrDfa          dfa,
            GrammarAnalysis grammar,
            ParserConflictResolver conflictResolver)
        {
            this.grammar = grammar;
            this.conflictResolver = conflictResolver;
            this.actionTable = new MutableTable<ParserInstruction>(
                                dfa.States.Length,
                                grammar.TotalSymbolCount);

            FillDfaTable(dfa.States);
            FillConflictActions();
            FillAmbiguousTerminalActions(dfa.States);
        }

        public ParserRuntime TargetRuntime =>
            (transitionToConflict.Count != 0 || hasTerminalAmbiguities)
            ? ParserRuntime.Glr
            : ParserRuntime.Deterministic;

        public ParserConflictInfo[] Conflicts => transitionToConflict.Values.ToArray();

        public ITable<ParserInstruction> ParserActionTable => actionTable;

        private void FillConflictActions()
        {
            int i = 0;
            foreach (var conflict in transitionToConflict.Values)
            {
                actionTable.Set(
                    conflict.State,
                    conflict.Token,
                    ParserInstruction.Conflict(i++));
            }
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
            ParserInstruction currentAction = actionTable.Get(state, token);
            ParserInstruction resolvedAction;

            if (currentAction == default(ParserInstruction))
            {
                actionTable.Set(state, token, action);
            }
            else if (currentAction == action)
            {
                // Nothing to resolve
            }
            else if (conflictResolver.TryResolve(currentAction, action, token, out resolvedAction))
            {
                actionTable.Set(state, token, resolvedAction);
            }
            else
            {
                ParserConflictInfo conflict;
                var key = new TransitionKey(state, token);
                if (!transitionToConflict.TryGetValue(key, out conflict))
                {
                    conflict = new ParserConflictInfo(state, token);
                    transitionToConflict[key] = conflict;
                    conflict.AddAction(currentAction);
                }

                if (!conflict.Actions.Contains(action))
                {
                    conflict.AddAction(action);
                }
            }
        }

        private void FillAmbiguousTerminalActions(DotState[] states)
        {
            for (int i = 0; i != states.Length; ++i)
            {
                var state = states[i];

                foreach (var ambToken in grammar.AmbiguousSymbols)
                {
                    var validTokenActions = new Dictionary<int,ParserInstruction>();
                    foreach (int token in ambToken.Alternatives)
                    {
                        var action = actionTable.Get(i, token);
                        if (action == default(ParserInstruction))
                        {
                            continue;
                        }

                        validTokenActions.Add(token, action);
                    }

                    switch (validTokenActions.Values.Distinct().Count())
                    {
                        case 0:
                            // AmbToken is entirely non-acceptable for this state
                            actionTable.Set(i, ambToken.EnvelopeIndex, ParserInstruction.FailAction);
                            break;
                        case 1:
                            {
                                var pair = validTokenActions.First();
                                if (pair.Key == ambToken.MainToken)
                                {
                                    // ambToken action is the same as for the main token
                                    actionTable.Set(i, ambToken.EnvelopeIndex, pair.Value);
                                }
                                else
                                {
                                    // Resolve ambToken to a one of the underlying tokens.
                                    // In runtime transition will be acceptable when this token
                                    // is in Msg and non-acceptable when this particular token
                                    // is not in Msg.
                                    var action = new ParserInstruction { Operation = ParserOperation.Resolve, Argument = pair.Key };
                                    actionTable.Set(i, ambToken.EnvelopeIndex, action);
                                }
                            }

                            break;
                        default:
                            // GLR parser is required to handle terminal token alternatives.
                            this.hasTerminalAmbiguities = true;
                            break;
                    }
                }
            }
        }
    }
}
