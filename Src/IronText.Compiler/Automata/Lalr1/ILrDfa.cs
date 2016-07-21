using System;

namespace IronText.Automata.Lalr1
{
    interface ILrDfa
    {
        LrTableOptimizations Optimizations { get; }

        DotState[] States { get; }
    }

    static class LrDfaExtensions
    {
        public static int[] GetStateToSymbolTable(this ILrDfa lrDfa)
        {
            return Array.ConvertAll(lrDfa.States, state => state.GetStateToken());
        }
    }
}
