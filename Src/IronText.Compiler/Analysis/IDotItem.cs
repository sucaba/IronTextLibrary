using System.Collections.Generic;
using IronText.Algorithm;

namespace IronText.Compiler.Analysis
{
    public interface IDotItem
    {
        bool IsAugmented { get; }
        bool IsKernel { get; }
        bool IsReduce { get; }
        MutableIntSet LA { get; set; }
        IEnumerable<int> NextTokens { get; }
        int Outcome { get; }
        int Position { get; }
        int PreviousToken { get; }
        int ProductionId { get; }

        DotItem CreateNextItem(int token);
        bool TryCreateNext(int nextToken, out DotItem outcome);
    }
}