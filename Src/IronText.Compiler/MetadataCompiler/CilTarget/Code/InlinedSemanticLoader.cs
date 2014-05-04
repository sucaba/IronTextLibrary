using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Lib.IL;
using IronText.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace IronText.MetadataCompiler
{
    class InlinedSemanticLoader : ISemanticLoader
    {
        private readonly ISemanticLoader                fallback;
        private readonly Production                   extendedProduction;
        private readonly List<InlinedSemanticBinding> semanticBindings;

        public InlinedSemanticLoader(
            ISemanticLoader     fallback,
            Production        extendedProduction)
        {
            this.fallback 		    = fallback;
            this.extendedProduction = extendedProduction;
            this.semanticBindings   = new List<InlinedSemanticBinding>();
            CollectInlinedSemanticBindings(extendedProduction, semanticBindings);
        }

        public bool LdSemantic(SemanticRef reference)
        {
            return fallback.LdSemantic(reference);
        }

        private static void CollectInlinedSemanticBindings(Production prod, List<InlinedSemanticBinding> output)
        {
            if (!prod.IsExtended)
            {
                return;
            }

            int pos = 0;
            foreach (var comp in EnumerateAllComponents(prod))
            {
                CollectInlinedSemanticBindings(prod, comp, pos++, output);
            }
        }

        private static IEnumerable<IProductionComponent> EnumerateAllComponents(IProductionComponent comp)
        {
            return Graph.BreadthFirst(comp, p => p.Components);
        }

        private static void CollectInlinedSemanticBindings(
            Production            owningProd,
            IProductionComponent  parentComponent,
            int                   componentPos,
            List<InlinedSemanticBinding> output)
        {
            int size = parentComponent.Size;
            var providers = new List<SemanticScope>();
            for (int i = 0; i != size; ++i)
            {
                var child = parentComponent.Components[i];

                var asSymbol = child as Symbol;
                if (asSymbol != null && asSymbol.LocalScope.Any())
                {
                    providers.Add(asSymbol.LocalScope);
                }
                else
                {
                    var asProduction = (Production)child;
                    var context = asProduction.ContextRef;
                    if (context != SemanticRef.None)
                    {
                        var closestProvidingScope = providers.LastOrDefault(s => s.Lookup(context));
                        if (closestProvidingScope != null)
                        {
                            output.Add(
                                new InlinedSemanticBinding
                                {
                                    Scope             = closestProvidingScope,
                                    Reference         = context,
                                    OwningProduction  = owningProd,
                                    ProvidingPosition = componentPos,
                                    Lookback          = i - providers.IndexOf(closestProvidingScope)
                                });
                        }
                    }
                }
            }
        }
    }
}
