using IronText.Compilation;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Reflection;
using IronText.Reflection.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.MetadataCompiler.CilTarget
{
    class ExtendedProductionCompiler : IProductionComponentVisitor
    {
        private readonly Fluent<EmitSyntax> emitCoder;
        private readonly VarsStack varsStack;
        private Production parentProduction;

        public ExtendedProductionCompiler(Fluent<EmitSyntax> emitCoder, VarsStack varsStack)
        {
            this.emitCoder = emitCoder;
            this.varsStack = varsStack;
        }

        public void Execute(Production extended)
        {
            ((IProductionComponent)extended).Accept(this);
        }

        void IProductionComponentVisitor.VisitSymbol(Symbol symbol)
        {
        }

        void IProductionComponentVisitor.VisitProduction(Production production)
        {
            var savedParentProd = this.parentProduction;
            this.parentProduction = production;

            int indexInParent = 0;
            foreach (var component in production.Components)
            {
                component.Accept(this);
                ++indexInParent;
            }

            this.parentProduction = savedParentProd;
            if (parentProduction != null)
            {
                ISemanticLoader globals = null;
                Fluent<IActionCode> coder = Fluent.Create(CreateActionCode(
                                                emitCoder,
                                                parentProduction,
                                                indexInParent,
                                                production,
                                                varsStack,
                                                globals));
            
                ProductionActionGenerator.CompileProduction(coder, varsStack, production);
            }
        }

        private static IActionCode CreateActionCode(
            Fluent<EmitSyntax> emitCoder,
            Production      parentProduction,
            int             indexInParent,
            Production      production,
            VarsStack       varsStack,
            ISemanticLoader globals)
        {
            throw new NotImplementedException();
        }
    }
}
