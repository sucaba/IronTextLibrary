using IronText.Algorithm;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    interface ILrTableBuilder
    {
        ITable<ParserDecision> GetResult();

        void AssignAccept(int state);
        void AssignReduce(int state, int token, int productionId);
        void AssignShift(int state, int token, int nexState);
        bool TryAssignResolution(int state, int ambiguousToken, int[] alternateTokens);
    }
}