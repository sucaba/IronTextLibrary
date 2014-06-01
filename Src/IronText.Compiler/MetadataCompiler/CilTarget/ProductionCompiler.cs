using IronText.Compilation;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Reflection;
using IronText.Reflection.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Misc;

namespace IronText.MetadataCompiler.CilTarget
{
    class ProductionCompiler
    {
        private readonly Fluent<EmitSyntax> emitCoder;
        private readonly VarsStack          varsStack;
        private readonly ISemanticLoader    globals;
        private Production                  parentProduction;

        public ProductionCompiler(
            Fluent<EmitSyntax> emitCoder,
            VarsStack          varsStack,
            ISemanticLoader    globals)
        {
            this.emitCoder = emitCoder;
            this.varsStack = varsStack;
            this.globals   = globals;
        }

        public void Execute(Production extended)
        {
            ProcessProduction(extended, -1, 0);
        }

        void ProcessComponent(IProductionComponent component, int indexInParent, int varsStackStart)
        {
            Production production;
            if (component.Match(out production))
            {
                ProcessProduction(production, indexInParent, varsStackStart);
            }
        }

        void ProcessProduction(Production production, int indexInParent, int varsStackStart)
        {
            var savedParentProd = this.parentProduction;
            this.parentProduction = production;

            int localIndexInParent = 0;
            foreach (var component in production.Components)
            {
                ProcessComponent(component, localIndexInParent, varsStackStart + localIndexInParent);
                ++localIndexInParent;
            }

            this.parentProduction = savedParentProd;
            if (parentProduction != null)
            {
                Fluent<IActionCode> coder = Fluent.Create(CreateActionCode(
                                                emitCoder,
                                                parentProduction,
                                                indexInParent,
                                                production,
                                                varsStack,
                                                varsStackStart,
                                                globals));
            
                ProductionActionGenerator.CompileProduction(
                                                coder,
                                                varsStack,
                                                varsStackStart,
                                                production);
            }
        }

        private static IActionCode CreateActionCode(
            Fluent<EmitSyntax> emitCoder,
            Production      parentProduction,
            int             indexInParent,
            Production      childProduction,
            VarsStack       varsStack,
            int             varsStackStart,
            ISemanticLoader globals)
        {
            var semanticLoader = new InlinedSemanticLoader(
                emitCoder,
                globals,
                varsStack,
                varsStackStart,
                parentProduction,
                indexInParent,
                childProduction);

            var result = new ProductionCode(
                emitCoder,
                semanticLoader,
                varsStack,
                varsStackStart);

            return result;
        }
    }
}
