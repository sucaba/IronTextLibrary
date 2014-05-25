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
    class ProductionCompiler : IProductionComponentVisitor, IActionCode
    {
        private readonly Fluent<IActionCode> coder;
        private readonly VarsStack localsStack;

        public ProductionCompiler(Fluent<IActionCode> coder)
        {
            this.coder = coder;
            this.localsStack = new VarsStack(ILCoder);

#if false
            this.contextCode = new ContextCode(
                                emit,
                                il => il.Ldarg(args[0]),
                                null,
                                data,
                                data.Grammar.Globals);
#endif
        }

        private void ILCoder(Pipe<EmitSyntax> pipe)
        {
            coder.Do(c => c.Emit(pipe));
        }

        public void Execute(IProductionComponent root)
        {
            var prod = (Production)root;
            ((IProductionComponentVisitor)this).VisitProduction(prod);
#if false
            int size = root.Size;
            for (int i = size; i != 0; --i)
            {
                localsStack.Push();
            }

            root.Accept(this);
#endif

        }

        void IProductionComponentVisitor.VisitSymbol(Symbol symbol)
        {
            throw new NotSupportedException(
                "Internal error: Production compiler can be used only for extended productions.");
        }

        void IProductionComponentVisitor.VisitProduction(Production production)
        {
#if false
            foreach (var component in production.Components)
            {
                component.Accept(this);
            }
#endif

            var bindings = production.Joint.All<CilProduction>();
            if (!bindings.Any())
            {
                coder.Do(c => c
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
                        coder.Do(c => c
                            .Emit(il => il.Pop()));
                    }

                    coder.Do(c => c
                        .LdSemantic(binding.Context.UniqueName)
                        .Do(binding.ActionBuilder))
                        ;
                }
            }

#if false
            localsStack.Pop(production.Size);
            localsStack.Push();
#endif
        }

        IActionCode IActionCode.Emit(Pipe<EmitSyntax> pipe)
        {
            ILCoder(pipe);
            return this;
        }

        IActionCode IActionCode.LdSemantic(string contextName)
        {
            throw new NotImplementedException();
            // contextCode.LdContext(contextName);
            // return code;
        }

        public IActionCode LdActionArgument(int index)
        {
            localsStack.LdSlot(index);
            return this;
        }

        IActionCode IActionCode.LdActionArgument(int index, Type argType)
        {
            localsStack.LdSlot(index);
            if (argType.IsValueType)
            {
                ILCoder(emit => emit.Unbox_Any(emit.Types.Import(argType)));
            }

            return this;
        }

        IActionCode IActionCode.LdMergerOldValue()
        {
            throw new NotImplementedException();
        }

        IActionCode IActionCode.LdMergerNewValue()
        {
            throw new NotImplementedException();
        }

        IActionCode IActionCode.LdMatcherTokenString()
        {
            throw new NotSupportedException();
        }

        IActionCode IActionCode.ReturnFromAction()
        {
            return this;
        }

        IActionCode IActionCode.SkipAction()
        {
            throw new NotSupportedException();
        }

        private void EmitCode(Pipe<EmitSyntax> fragment)
        {
            ILCoder(fragment);
        }
    }
}
