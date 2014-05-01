using IronText.Framework;
using IronText.Reflection;
using IronText.Reflection.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.MetadataCompiler.CilTarget
{
    class ProductionCompiler : IProductionComponentVisitor
    {
        private readonly Action<Pipe<IActionCode>> coder;

        public ProductionCompiler(Action<Pipe<IActionCode>> coder)
        {
            this.coder = coder;
        }

        public void Execute(IProductionComponent root)
        {
            root.Accept(this);
        }

        void IProductionComponentVisitor.VisitSymbol(Symbol symbol)
        {
        }

        void IProductionComponentVisitor.VisitProduction(Production production)
        {
            var bindings = production.Joint.All<CilProduction>();
            if (!bindings.Any())
            {
                coder(c => c
                    .Emit(il => il.Ldnull()));
            }
            else
            {
                bool first = true;
                foreach (var binding in bindings)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        // Result of this rule supersedes result of the prvious one
                        coder(c => c
                            .Emit(il => il.Pop()));
                    }

                    coder(c => c
                        .Do(binding.Context.Load)
                        .Do(binding.ActionBuilder))
                        ;
                }
            }
        }
    }
}
