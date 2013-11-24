using System;
using IronText.Compiler;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Framework.Reflection;

namespace IronText.Automata.Lalr1
{
    interface ILrDfa
    {
        EbnfGrammarAnalysis Grammar { get; }

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
