using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    interface ILrTableBuilder
    {
        ITable<ParserDecision> GetResult();

        void AssignAccept(DotState state);
        void AssignReduce(DotState state, DotItem item, int token);
        void AssignShift(DotState state, DotItemTransition transition);
        bool TryAssignResolution(DotState state, int ambiguousToken, int[] alternateTokens);
    }
}