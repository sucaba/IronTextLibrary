using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Collections;
using IronText.Reporting;
using IronText.Runtime;
using System;
using System.Collections.Generic;

namespace IronText.MetadataCompiler
{
    class ParserConflictProvider
    {
        public ParserConflictProvider(CanonicalLrDfaTable lrTable)
        {
            this.Conflicts = FillConflictActions(lrTable.DecisionTable);
        }

        public ParserConflictInfo[] Conflicts { get; }

        private static ParserConflictInfo[] FillConflictActions(ITable<ParserDecision> decisionTable)
        {
            var result = new List<ParserConflictInfo>();

            for (int state = 0; state != decisionTable.RowCount; ++state)
            for (int token = 0; token != decisionTable.ColumnCount; ++token)
                {
                    var decision = decisionTable.Get(state, token);
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

            foreach (var alternative in decision.AllAlternatives())
            {
                result.AddAction(decision.Instruction);
            }

            return result;
        }
    }
}
