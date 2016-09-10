using IronText.Algorithm;
using IronText.Runtime;
using IronText.Collections;
using System.Collections.Generic;

namespace IronText.Automata.Lalr1
{
    class LrTableBuilder
    {
        private readonly ParserConflictResolver conflictResolver;
        private MutableTable<ParserDecision> decisionTable;

        public LrTableBuilder(
            ParserConflictResolver conflictResolver,
            int stateCount,
            int tokenCount)
        {
            this.conflictResolver = conflictResolver;
            this.decisionTable = new MutableTable<ParserDecision>(
                                    stateCount,
                                    tokenCount);
        }

        public Dictionary<int, ParserDecision> GetMultiple(int state, int[] tokens)
        {
            var result = new Dictionary<int, ParserDecision>();

            foreach (int token in tokens)
            {
                var decision = decisionTable.Get(state, token);
                if (decision != ParserDecision.NoAlternatives)
                {
                    result.Add(token, decision);
                }
            }

            return result;
        }

        public void AssignAction(int state, int token, ParserInstruction action)
        {
            AssignAction(state, token, new ParserDecision(action));
        }

        public void AssignAction(int state, int token, ParserDecision decision)
        {
            ParserDecision current = decisionTable.Get(state, token);
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

            decisionTable.Set(state, token, resolved);
        }

        public ITable<ParserDecision> GetResult()
        {
            return decisionTable;
        }
    }
}