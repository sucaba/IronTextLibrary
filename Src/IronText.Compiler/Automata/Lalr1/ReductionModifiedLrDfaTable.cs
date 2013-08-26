using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Framework;

namespace IronText.Automata.Lalr1
{
    class ReductionModifiedLrDfaTable : ILrParserTable
    {
        private readonly BnfGrammar grammar;
        private readonly Dictionary<TransitionKey, ParserConflictInfo> transitionToConflict 
            = new Dictionary<TransitionKey, ParserConflictInfo>();
        private readonly IMutableTable<int> actionTable;
        private int[]  conflictActionTable;

        public ReductionModifiedLrDfaTable(ILrDfa dfa)
        {
            this.grammar = dfa.Grammar;
            var states = dfa.States;
            this.actionTable = new MutableTable<int>(states.Length, dfa.Grammar.TokenCount);
            FillDfaTable(states);
            BuildConflictActionTable();
        }

        public int[] GetConflictActionTable() { return conflictActionTable; }

        public ITable<int> GetParserActionTable() { return actionTable; }

        private void BuildConflictActionTable()
        {
            var conflictList = new List<int>();
            foreach (var conflict in transitionToConflict.Values)
            {
                var refAction = new ParserAction
                            {
                                Kind = ParserActionKind.Conflict,
                                Value1 = conflictList.Count,
                                Size = (short)conflict.Actions.Count
                            };
                             
                actionTable.Set(conflict.State, conflict.Token, ParserAction.Encode(refAction));
                foreach (var action in conflict.Actions)
                {
                    conflictList.Add(ParserAction.Encode(action)); 
                }
            }

            this.conflictActionTable = conflictList.ToArray();
        }

        private void FillDfaTable(DotState[] states)
        {
            for (int i = 0; i != states.Length; ++i)
            {
                var state = states[i];

                foreach (var item in state.Items)
                {
                    var rule = item.Rule;

                    if (rule.Parts.Length != item.Pos)
                    {
                        int nextToken = rule.Parts[item.Pos];

                        var action = new ParserAction
                            { 
                                Kind = ParserActionKind.Shift,
                                State = state.GetNextIndex(nextToken)
                            };

                        AddAction(i, nextToken, action);
                    }

                    bool isStartRule = rule.Left == BnfGrammar.AugmentedStart;

                    if (item.Pos == rule.Parts.Length || grammar.IsTailNullable(rule.Parts, item.Pos))
                    {
                        ParserAction action;

                        if (isStartRule)
                        {
                            action = new ParserAction { Kind = ParserActionKind.Accept };
                        }
                        else
                        {
                            action = new ParserAction
                            {
                                Kind = ParserActionKind.Reduce,
                                Rule = item.RuleId,
                                Size = (short)item.Pos
                            };
                        }

                        foreach (var lookahead in item.Lookaheads)
                        {
                            AddAction(i, lookahead, action);
                        }
                    }
                }
            }

#if SWITCH_FEATURE
            // Now assign switch actions in all states which will not cause conflicts
            for (int i = 0; i != states.Length; ++i)
            {
                var state = states[i];

                foreach (var item in state.Items)
                {
                    var rule = item.Rule;

                    if (rule.Parts.Length != item.Pos && grammar.IsExternal(rule.Parts[item.Pos]))
                    {
                        var action = new ParserAction
                        {
                            Kind = ParserActionKind.Switch,
                            ExternalToken = rule.Parts[item.Pos]
                        };

                        foreach (var token in grammar.EnumerateTokens())
                        {
                            // TODO: Test that ignored switches do not cause
                            //       action invocation errors
                            // TODO: Emit warning or error on ignored switches

                            if (actionTable.Get(i, token) == 0)
                            {
                                AddAction(i, token, action);
                            }
                        }
                    }
                }
            }
#endif
        }

        private void AddAction(int state, int token, ParserAction action)
        {
            int cell = ParserAction.Encode(action);
            int currentCell = actionTable.Get(state, token);
            if (currentCell == 0)
            {
                actionTable.Set(state, token, cell);
            }
            else if (currentCell != cell)
            {
                int resolvedCell;
                if (!TryResolveShiftReduce(currentCell, cell, token, out resolvedCell))
                {
                    ParserConflictInfo conflict;
                    var key = new TransitionKey(state, token);
                    if (!transitionToConflict.TryGetValue(key, out conflict))
                    {
                        conflict = new ParserConflictInfo { State = state, Token = token };
                        transitionToConflict[key] = conflict;
                        conflict.Actions.Add(ParserAction.Decode(currentCell));
                    }

                    if (!conflict.Actions.Contains(action))
                    {
                        conflict.Actions.Add(action);
                    }
                }

                actionTable.Set(state, token, resolvedCell);
            }
        }

        private bool TryResolveShiftReduce(int actionX, int actionY, int incomingToken, out int output)
        {
            output = 0;

            int shiftAction, reduceAction;
            if (ParserAction.GetKind(actionX) == ParserActionKind.Shift
                && ParserAction.GetKind(actionY) == ParserActionKind.Reduce)
            {
                shiftAction = actionX;
                reduceAction = actionY;
            }
            else if (ParserAction.GetKind(actionY) == ParserActionKind.Shift
                && ParserAction.GetKind(actionX) == ParserActionKind.Reduce)
            {
                shiftAction = actionY;
                reduceAction = actionX;
            }
            else
            {
#if LALR1_TOLERANT
                // Unsupported conflict type. Use first action
                output = actionX;
#else
                output = ParserAction.Encode(ParserActionKind.Conflict, 0);
#endif
                return false;
            }

            var shiftTokenPrecedence = grammar.GetTermPrecedence(incomingToken);
            var reduceRulePrecedence = grammar.GetRulePrecedence(ParserAction.GetId(reduceAction));

            if (shiftTokenPrecedence == null && reduceRulePrecedence == null)
            {
#if LALR1_TOLERANT
                // In case of conflict prefer shift over reduce
                output = shiftAction;
#else
                output = ParserAction.Encode(ParserActionKind.Conflict, 0);
#endif
                return false;
            }
            else if (shiftTokenPrecedence == null)
            {
                output = reduceAction;
            }
            else if (reduceRulePrecedence == null)
            {
                output = shiftAction;
            }
            else if (Precedence.IsReduce(reduceRulePrecedence, shiftTokenPrecedence))
            {
                output = reduceAction;
            }
            else
            {
                output = shiftAction;
            }

            return true;
        }
    }
}
