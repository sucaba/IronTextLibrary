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
        private readonly ISemanticLoader              fallback;
        private readonly List<InlinedSemanticBinding> semanticBindings;
        private readonly Production             	  inlinedProduction;
        private readonly int                          parentPosition;

        public InlinedSemanticLoader(
            ISemanticLoader   fallback,
            Production        owningProduction,
            Production        inlinedProduction,
            int               parentPosition)
        {
            this.fallback 		   = fallback;
            this.inlinedProduction = inlinedProduction;
            this.parentPosition    = parentPosition;
            this.semanticBindings  = new List<InlinedSemanticBinding>();

            //CollectInlinedSemanticBindings(owningProduction, semanticBindings);
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
            var allComponents = Graph.BreadthFirst<IProductionComponent>(prod, p => p.Components);
            foreach (var comp in allComponents)
            {
                CollectInlinedSemanticBindings(prod, comp, pos++, output);
            }
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
