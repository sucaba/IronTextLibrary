using System;
using IronText.Runtime;

namespace IronText.Reflection
{
    [Serializable]
    public class RuntimeProductionNode
    {
        public RuntimeProductionNode(int outcome)
            : this(outcome, null)
        {
        }

        public RuntimeProductionNode(
            int outcome,
            RuntimeProductionNode[] components)
        {
            this.Outcome = outcome;
            this.Components = components;
        }

        public int                     Outcome    { get; }

        public RuntimeProductionNode[] Components { get; }
    }

    public static class ProductionExtensions
    {
        public static RuntimeProduction ToRuntime(this Production production) =>
            new RuntimeProduction(
                production.Index,
                production.Outcome.Index,
                production.InputTokens,
                MakeNode(production));

        static RuntimeProductionNode MakeTree(IProductionComponent component) =>
            MakeNode((dynamic)component);

        static RuntimeProductionNode MakeNode(Production production) =>
            new RuntimeProductionNode(
                production.Outcome.Index,
                Array.ConvertAll(production.ChildComponents, MakeTree));

        static RuntimeProductionNode MakeNode(Symbol symbol) =>
            new RuntimeProductionNode(symbol.Index);

        static RuntimeProductionNode MakeNode(IProductionComponent other)
        {
            throw new InvalidOperationException($"Unknown component type '{other.GetType().FullName}'");
        }
    }
}
