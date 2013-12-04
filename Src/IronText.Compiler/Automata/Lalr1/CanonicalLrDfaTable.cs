using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Framework;
using System.Collections.ObjectModel;
using IronText.Framework.Reflection;
using IronText.Compiler;
using IronText.Compiler.Analysis;

namespace IronText.Automata.Lalr1
{
    class CanonicalLrDfaTable : ILrParserTable
    {
        private readonly EbnfGrammarAnalysis grammar;
        private readonly Dictionary<TransitionKey, ParserConflictInfo> transitionToConflict 
            = new Dictionary<TransitionKey, ParserConflictInfo>();

        private IMutableTable<int> actionTable;
        private int[]              conflictActionTable;
        private readonly bool      canOptimizeReduceStates;

        public CanonicalLrDfaTable(ILrDfa dfa, IMutableTable<int> actionTable)
        {
            var flag = LrTableOptimizations.EliminateLr0ReduceStates;
            this.canOptimizeReduceStates = (dfa.Optimizations & flag) == flag;

            this.grammar = dfa.Grammar;
            this.actionTable = actionTable ?? new MutableTable<int>(dfa.States.Length, grammar.Symbols.Count);
            FillDfaTable(dfa.States);
            BuildConflictTable();
        }

        public bool RequiresGlr { get; private set; }

        public ParserConflictInfo[] Conflicts
        {
            get { return transitionToConflict.Values.ToArray(); }
        }

        public int[] GetConflictActionTable()
        {
            return conflictActionTable;        
        }

        private void BuildConflictTable()
        {
            var conflictList = new List<int>();
            foreach (var conflict in transitionToConflict.Values)
            {
                var refAction = new ParserAction
                            {
                                Kind   = ParserActionKind.Conflict,
                                Value1 = conflictList.Count,
                                Size   = (short)conflict.Actions.Count
                            };
                             
                actionTable.Set(conflict.State, conflict.Token, ParserAction.Encode(refAction));
                foreach (var action in conflict.Actions)
                {
                    conflictList.Add(ParserAction.Encode(action)); 
                }
            }

            this.conflictActionTable = conflictList.ToArray();
        }

        public ITable<int> GetParserActionTable() { return this.actionTable; }

        private void FillDfaTable(DotState[] states)
        {
            for (int i = 0; i != states.Length; ++i)
            {
                var state = states[i];
                Debug.Assert(i == state.Index);

                foreach (var item in state.Items)
                {
                    if (!item.IsReduce)
                    {
                        int nextToken = item.NextToken;

                        if (canOptimizeReduceStates
                            && item.IsShiftReduce
                            && !state.Transitions.Exists(t => t.Tokens.Contains(nextToken)))
                        {
                            var action = new ParserAction
                            {
                                Kind = ParserActionKind.ShiftReduce,
                                ProductionId = item.ProductionId
                            };

                            AssignAction(i, nextToken, action);
                        }
                        else
                        {
                            var action = new ParserAction
                            {
                                Kind = ParserActionKind.Shift,
                                State = state.GetNextIndex(nextToken)
                            };

                            AssignAction(i, nextToken, action);
                        }
                    }
                    else if (item.IsAugmented)
                    {
                        var action = new ParserAction { Kind = ParserActionKind.Accept };
                        AssignAction(i, EbnfGrammar.Eoi, action);
                    }
                    else
                    {
                        var action = new ParserAction { Kind = ParserActionKind.Reduce, ProductionId = item.ProductionId };
                        foreach (var lookahead in item.LA)
                        {
                            AssignAction(i, lookahead, action);
                        }
                    }
                }
            }
        }

        private void AssignAction(int state, int token, ParserAction action)
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
                    RequiresGlr = true;

                    ParserConflictInfo conflict;
                    var key = new TransitionKey(state, token);
                    if (!transitionToConflict.TryGetValue(key, out conflict))
                    {
                        conflict = new ParserConflictInfo(state, token);
                        transitionToConflict[key] = conflict;
                        conflict.AddAction(currentCell);
                    }

                    if (!conflict.Actions.Contains(action))
                    {
                        conflict.AddAction(action);
                    }
                }

                actionTable.Set(state, token, resolvedCell);
            }
        }

        private bool TryResolveShiftReduce(int actionX, int actionY, int incomingToken, out int output)
        {
            output = 0;

            int shiftAction, reduceAction;
            if (ParserAction.IsShift(actionX)
                && ParserAction.GetKind(actionY) == ParserActionKind.Reduce)
            {
                shiftAction = actionX;
                reduceAction = actionY;
            }
            else if (ParserAction.IsShift(actionY)
                && ParserAction.GetKind(actionX) == ParserActionKind.Reduce)
            {
                shiftAction = actionY;
                reduceAction = actionX;
            }
            else
            {
                // Unsupported conflict type. Use first action
                output = actionX;
                return false;
            }

            var shiftTokenPrecedence = grammar.GetTermPrecedence(incomingToken);
            var reduceRulePrecedence = grammar.GetProductionPrecedence(ParserAction.GetId(reduceAction));

            if (shiftTokenPrecedence == null && reduceRulePrecedence == null)
            {
                // In case of conflict prefer shift over reduce
                output = shiftAction;
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
