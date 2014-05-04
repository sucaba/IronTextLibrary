#if false
using System.Linq;
using IronText.Extensibility;
using IronText.Reflection;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.MetadataCompiler
{
    class InlinedSemanticCode : ISemanticCode
    {
        private EmitSyntax                            emit;
        private readonly ISemanticCode                fallback;
        private readonly List<InlinedSemanticBinding> semanticBindings;
        private readonly Production                   extendedProduction;
        private readonly Pipe<EmitSyntax>             ldGlobalScope;
        private readonly SemanticScope                globals;

        public InlinedSemanticCode(
            ISemanticCode     fallback,
            Production        extendedProduction)
        {
            this.fallback 		    = fallback;
            this.extendedProduction = extendedProduction;
            this.semanticBindings   = new List<InlinedSemanticBinding>();
            CollectInlinedSemanticBindings(extendedProduction, semanticBindings);
        }

        public void LdSemantic(string name)
        {
            fallback.LdSemantic(name);
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
            List<SemanticBinding> output)
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


        private bool LdGlobal(SemanticRef reference)
        {
            var value = globals.Resolve(reference);
            if (value == null)
            {
                return false;
            }

            var binding = value.Joint.The<CilSemanticValue>();
            this.emit = binding.Ld(emit, ldGlobalScope);
            return true;
        }
    }
}
#endif
