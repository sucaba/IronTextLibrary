using IronText.Algorithm;
using IronText.Runtime;
using IronText.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class TurnTableBuilder
    {
        private readonly TurnConflictResolver conflictResolver;
        private readonly MutableTable<Turn>   turnTable;

        public TurnTableBuilder(
            TurnConflictResolver precedenceProvider,
            int stateCount,
            int tokenCount)
        {
            this.conflictResolver = precedenceProvider;
            this.turnTable = new MutableTable<Turn>(
                                    stateCount,
                                    tokenCount);
        }

        public ITable<Turn> GetResult()
        {
            return turnTable;
        }

#if false
        public bool TryAssignResolution(
            int   state,
            int   ambiguousToken,
            int[] alternateTokens)
        {
            int  resolvedToken;
            Turn resolvedDecision;

            if (!TryResolve(
                state,
                alternateTokens,
                out resolvedToken,
                out resolvedDecision))
            {
                return false;
            }

            // We need to pick proper term data from <see cref="Message" />.
            if (resolvedToken == alternateTokens[0])
            {
                // For a main token scanner should pass a data as a first
                // (=default) item. This way resolution (=picking proper item 
                // from term-data list alternatives) is needed only for 
                // non-main terms.
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
            int      state,
            int[]    alternateTokens,
            out int  resolvedToken,
            out Turn resolvedDecision)
        {
            var tokenToDecision = GetMultiple(state, alternateTokens);

            int count = tokenToDecision.Values.Distinct().Count();
            switch (count)
            {
                case 0:
                    // Resolve as a failure for a main token
                    resolvedToken = alternateTokens[0];
                    resolvedDecision = Turn.NoAlternatives;
                    break;
                case 1:
                    resolvedToken = tokenToDecision.First().Key;
                    resolvedDecision = tokenToDecision.First().Value;
                    break;
                default:
                    resolvedToken = -1;
                    resolvedDecision = Turn.NoAlternatives;
                    break;
            }

            return count <= 1;
        }

        private Dictionary<int, Turn> GetMultiple(int state, int[] tokens)
        {
            var result = new Dictionary<int, Turn>();

            foreach (int token in tokens)
            {
                var decision = turnTable.Get(state, token);
                if (decision != Turn.NoAlternatives)
                {
                    result.Add(token, decision);
                }
            }

            return result;
        }
#endif

        public void AssignShift(
            int state,
            int token,
            int nextState)
        {
            AssignAction(
                state,
                token,
                ParserInstruction.Shift(nextState));
        }

        public void AssignAccept(int state)
        {
            AssignAction(
                state,
                PredefinedTokens.Eoi,
                ParserInstruction.AcceptAction);
        }

        public void AssignReturn(int state, int token, int producedToken)
        {
            AssignAction(
                state,
                token,
                ParserInstruction.Return(producedToken));
        }

        public void AssignReduce(int state, int token, int productionId, int nextState)
        {
            AssignAction(
                state,
                token,
                ParserInstruction.Reduce(productionId));
        }

        private void AssignAction(int state, int token, ParserInstruction instruction)
        {
            AssignAction(state, token, instruction);
        }
    }
}