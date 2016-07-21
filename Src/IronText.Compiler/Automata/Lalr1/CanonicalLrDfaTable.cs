using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class CanonicalLrDfaTable : ILrParserTable
    {
        private readonly GrammarAnalysis grammar;
        private readonly Dictionary<TransitionKey, ParserConflictInfo> transitionToConflict 
            = new Dictionary<TransitionKey, ParserConflictInfo>();
        private readonly IMutableTable<ParserAction> actionTable;
        private ParserAction[]  conflictActionTable;

        public CanonicalLrDfaTable(
            ILrDfa dfa,
            IMutableTable<ParserAction> actionTable)
        {
            this.grammar = dfa.GrammarAnalysis;
            this.actionTable = actionTable ?? new MutableTable<ParserAction>(
                                                dfa.States.Length,
                                                grammar.TotalSymbolCount);

            FillDfaTable(dfa.States);
            BuildConflictTable();
        }

        public ParserRuntime TargetRuntime =>
            transitionToConflict.Count != 0
            ? ParserRuntime.Glr
            : ParserRuntime.Deterministic;

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

                        AssignAction(i, nextToken, action);
                    }
                    else if (item.IsAugmented)
                    {
                        var action = new ParserAction { Kind = ParserActionKind.Accept };
                        AssignAction(i, PredefinedTokens.Eoi, action);
                    }
                    else
                    {
                        var action = new ParserAction
                        {
                            Kind         = ParserActionKind.Reduce,
                            ProductionId = item.ProductionId,
                        };

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
            var currentAction = actionTable.Get(state, token);
            if (currentAction == default(ParserAction))
            {
                actionTable.Set(state, token, action);
            }
            else if (currentAction != action)
            {
                ParserAction resolvedAction;
                if (TryResolveShiftReduce(currentAction, action, token, out resolvedAction))
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
                output = ParserAction.FailAction;
                return false;
            }

            var shiftTokenPrecedence = grammar.GetTermPrecedence(incomingToken);
            var reduceRulePrecedence = grammar.GetProductionPrecedence(reduceAction.ProductionId);

            if (shiftTokenPrecedence == null && reduceRulePrecedence == null)
            {
                output = ParserAction.FailAction;
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
