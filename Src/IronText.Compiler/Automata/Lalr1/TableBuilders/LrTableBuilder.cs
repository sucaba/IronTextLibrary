using IronText.Algorithm;
using IronText.Runtime;
using IronText.Collections;
using System.Collections.Generic;
using IronText.Compiler.Analysis;
using System.Linq;

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

        public ITable<ParserDecision> GetResult()
        {
            return decisionTable;
        }

        public bool TryAssignResolution(
            DotState       state,
            int            ambiguousToken,
            int[]          alternateTokens)
        {
            int            resolvedToken;
            ParserDecision resolvedDecision;

            if (!TryResolve(
                state,
                alternateTokens,
                out resolvedToken,
                out resolvedDecision))
            {
                return false;
            }

            // We need to pick proper term data from <see cref="Message" />
            // and for a main token scanner should pass a data as a first
            // (=default) item.
            // This way resolution (=picking proper item from term-data 
            // list alternatives) is needed only for non-main terms.
            if (resolvedToken == alternateTokens[0])
            {
                AssignAction(
                    state,
                    ambiguousToken,
                    resolvedDecision);
            }
            else
            {
                AssignAction(
                    state,
                    ambiguousToken,
                    ParserInstruction.Resolve(resolvedToken));
            }

            return true;
        }
    
        private bool TryResolve(
            DotState state,
            int[] alternateTokens,
            out int resolvedToken,
            out ParserDecision resolvedDecision)
        {
            var tokenToDecision = GetMultiple(state, alternateTokens);

            int count = tokenToDecision.Values.Distinct().Count();
            switch (count)
            {
                case 0:
                    // Resolve as a failure for a first token
                    resolvedToken = alternateTokens[0];
                    resolvedDecision = ParserDecision.NoAlternatives;
                    break;
                case 1:
                    resolvedToken = tokenToDecision.First().Key;
                    resolvedDecision = tokenToDecision.First().Value;
                    break;
                default:
                    resolvedToken = -1;
                    resolvedDecision = ParserDecision.NoAlternatives;
                    break;
            }

            return count <= 1;
        }

        private Dictionary<int, ParserDecision> GetMultiple(DotState state, int[] tokens)
        {
            int stateIndex = state.Index;

            var result = new Dictionary<int, ParserDecision>();

            foreach (int token in tokens)
            {
                var decision = decisionTable.Get(stateIndex, token);
                if (decision != ParserDecision.NoAlternatives)
                {
                    result.Add(token, decision);
                }
            }

            return result;
        }

        public void AssignShift(DotState state, DotItemTransition transition)
        {
            AssignAction(
                state,
                transition.Token,
                ParserInstruction.Shift(
                    state.GetNextIndex(transition.Token)));
        }

        public void AssignAccept(DotState state)
        {
            AssignAction(
                state,
                PredefinedTokens.Eoi,
                ParserInstruction.AcceptAction);
        }

        public void AssignReduce(DotState state, DotItem item, int token)
        {
            AssignAction(
                state,
                token,
                ParserInstruction.Reduce(item.ProductionId));
        }

        private void AssignAction(DotState state, int token, ParserDecision action)
        {
            AssignAction(state.Index, token, action);
        }

        private void AssignAction(DotState state, int token, ParserInstruction action)
        {
            AssignAction(state.Index, token, action);
        }

        private void AssignAction(int state, int token, ParserInstruction action)
        {
            AssignAction(state, token, new ParserDecision(action));
        }

        private void AssignAction(int state, int token, ParserDecision decision)
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
    }
}