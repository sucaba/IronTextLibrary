using System;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Framework.Reflection;

namespace IronText.Automata.Lalr1
{
    public interface ILrDfa
    {
        IBuildtimeBnfGrammar Grammar { get; }

        LrTableOptimizations Optimizations { get; }

        DotState[] States { get; }
    }

    public static class LrDfaExtensions
    {
        public static int[] GetStateToSymbolTable(this ILrDfa lrDfa)
        {
            return Array.ConvertAll(lrDfa.States, state => state.GetStateToken());
        }
    }
}
