using IronText.Automata.Lalr1;
using IronText.Extensibility;
using IronText.Reflection;
using System.Collections.Generic;

namespace IronText.MetadataCompiler
{
    class SemanticBindingProvider
    {
        public SemanticBindingProvider(
            Grammar grammar,
            ILrDfa  lrDfa)
        {
            var output = new List<StackSemanticBinding>();
            var states     = lrDfa.States;
            int stateCount = states.Length;

            for (int parentState = 0; parentState != stateCount; ++parentState)
            {
                foreach (var item in states[parentState].Items)
                {
                    if (item.Position == 0 || item.IsReduce)
                    {
                        // Skip items which cannot provide semantic values.
                        continue;
                    }

                    var providingProd   = grammar.Productions[item.ProductionId];
                    var providingSymbol = providingProd.Input[0];
                    var childSymbol     = providingProd.Input[item.Position];

                    foreach (var consumingProd in childSymbol.Productions)
                    {
                        if (providingSymbol.LocalScope.Lookup(consumingProd.ContextRef))
                        {
                            output.Add(
                                new StackSemanticBinding
                                {
                                    StackState          = parentState,
                                    ProvidingProduction = providingProd,
                                    StackLookback       = item.Position,
                                    ConsumingProduction = consumingProd,
                                    Scope               = providingSymbol.LocalScope,
                                    Reference           = consumingProd.ContextRef
                                });
                        }
                    }
                }
            }

            Bindings = output;
        }

        public List<StackSemanticBinding> Bindings { get; }
    }
}
