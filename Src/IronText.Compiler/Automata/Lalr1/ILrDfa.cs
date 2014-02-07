using System;
using IronText.Compiler.Analysis;

namespace IronText.Automata.Lalr1
{
    interface ILrDfa
    {
        GrammarAnalysis GrammarAnalysis { get; }

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
