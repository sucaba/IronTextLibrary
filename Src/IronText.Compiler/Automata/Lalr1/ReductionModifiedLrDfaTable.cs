using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;
using System.Diagnostics;
using System;

namespace IronText.Automata.Lalr1
{
    class ReductionModifiedLrDfaTable : ILrParserTable
    {
        private readonly GrammarAnalysis grammar;
        private readonly Dictionary<TransitionKey, ParserConflictInfo> transitionToConflict 
            = new Dictionary<TransitionKey, ParserConflictInfo>();
        private readonly IMutableTable<ParserAction> actionTable;
        private ParserAction[]  conflictActionTable;

        public ReductionModifiedLrDfaTable(ILrDfa dfa, IMutableTable<ParserAction> actionTable = null)
        {
            this.grammar = dfa.GrammarAnalysis;

            this.actionTable = actionTable ?? new MutableTable<ParserAction>(
                                                dfa.States.Length,
                                                grammar.TotalSymbolCount);
            FillDfaTable(dfa.States);
            BuildConflictTable();
        }

        public bool RequiresGlr { get { return true; } }

        public ParserConflictInfo[] Conflicts
        {
            get { return transitionToConflict.Values.ToArray(); }
        }

        public ParserAction[] GetConflictActionTable() { return conflictActionTable; }

        public ITable<ParserAction> GetParserActionTable() { return actionTable; }

        private void BuildConflictTable()
        {
            var conflictList = new List<ParserAction>();
            foreach (var conflict in transitionToConflict.Values)
            {
                var refAction = new ParserAction
                                {
                                    Kind          = ParserActionKind.Conflict,
                                    Value1        = conflictList.Count,
                                    ConflictCount = (short)conflict.Actions.Count
                                };
                             
                actionTable.Set(conflict.State, conflict.Token, refAction);
                foreach (var action in conflict.Actions)
                {
                    conflictList.Add(action); 
                }
            }

            this.conflictActionTable = conflictList.ToArray();
        }

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

                        var action = new ParserAction
                        {
                            Kind  = ParserActionKind.Shift,
                            State = state.GetNextIndex(nextToken)
                        };

                        AddAction(i, nextToken, action);
                    }

                    bool isStartRule = item.IsAugmented;

                    bool isTailNullable = !isStartRule && !item.IsReduce && grammar.IsTailNullable(item);
                    if (isTailNullable)
                    {
                        // Ensure that tail-nullable productions where eliminated
                        throw new InvalidOperationException("Tail nullable productions are not supported");
                    }

                    if (item.IsReduce || isTailNullable)
                    {
                        ParserAction action;

                        if (isStartRule)
                        {
                            if (item.Position == 0)
                            {
                                continue;
                            }
                            else
                            {
                                action = new ParserAction { Kind = ParserActionKind.Accept };
                            }
                        }
                        else
                        {
                            action = new ParserAction
                            {
                                Kind         = ParserActionKind.Reduce,
                                ProductionId = item.ProductionId,
                            };
                        }

                        foreach (var lookahead in item.LA)
                        {
                            if (!IsValueOnlyEpsilonReduceItem(item, state, lookahead))
                            {
                                AddAction(i, lookahead, action);
                            }
                        }
                    }
                }
            }
        }

        private bool IsValueOnlyEpsilonReduceItem(DotItem item, DotState state, int lookahead)
        {
            if (item.Position != 0 
                || grammar.IsStartProduction(item.ProductionId)
                || !grammar.IsTailNullable(item)
                || !item.LA.Contains(lookahead))
            {
                return false;
            }

            int epsilonToken = item.Outcome;

            foreach (var parentItem in state.Items)
            {
                if (parentItem == item)
                {
                    continue;
                }

                if (parentItem.NextToken == epsilonToken)
                {
                    if (!grammar.IsTailNullable(parentItem))
                    {
                        // there is at least one rule which needs shift on epsilonToken
                        return false;
                    }

                    if (grammar.HasFirst(parentItem.CreateNextItem(), lookahead))
                    {
                        // One of the subseqent non-terms in parentItem can start parsing with current lookahead.
                        // It means that we need tested epsilonToken production for continue parsing on parentItem.
                        return false;
                    }
                }
            }

            return true;
        }

        private void AddAction(int state, int token, ParserAction action)
        {
            var currentCell = actionTable.Get(state, token);
            if (currentCell == default(ParserAction))
            {
                actionTable.Set(state, token, action);
            }
            else if (currentCell != action)
            {
                ParserAction resolvedCell;
                if (!TryResolveShiftReduce(currentCell, action, token, out resolvedCell))
                {
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

        private bool TryResolveShiftReduce(
            ParserAction actionX,
            ParserAction actionY,
            int incomingToken,
            out ParserAction output)
        {
            output = ParserAction.FailAction;

            ParserAction shiftAction, reduceAction;
            if (actionX.IsShiftAction
                && actionY.Kind == ParserActionKind.Reduce)
            {
                shiftAction = actionX;
                reduceAction = actionY;
            }
            else if (actionY.IsShiftAction
                && actionX.Kind == ParserActionKind.Reduce)
            {
                shiftAction = actionY;
                reduceAction = actionX;
            }
            else
            {
                output = new ParserAction(ParserActionKind.Conflict, 0);
                return false;
            }

            var shiftTokenPrecedence = grammar.GetTermPrecedence(incomingToken);
            var reduceRulePrecedence = grammar.GetProductionPrecedence(reduceAction.ProductionId);

            if (shiftTokenPrecedence == null && reduceRulePrecedence == null)
            {
                output = new ParserAction(ParserActionKind.Conflict, 0);
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
