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
        private readonly VarsStack varsStack;

        public ExtendedProductionCompiler(VarsStack varsStack)
        {
            this.varsStack = varsStack;
        }

        public void Execute(Production extended)
        {
            foreach (var component in extended.Components)
            {
                component.Accept(this);
            }
        }

        void IProductionComponentVisitor.VisitSymbol(Symbol symbol)
        {
            throw new NotSupportedException(
                "Internal error: Production compiler can be used only for extended productions.");
        }

        void IProductionComponentVisitor.VisitProduction(Production production)
        {
            Fluent<IActionCode> coder     = null;
            VarsStack           varsStack = null;
            ProductionActionGenerator.CompileProduction(coder, varsStack, production);
        }
    }
}
