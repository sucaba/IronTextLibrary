using IronText.Compilation;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Reflection;
using IronText.Reflection.Managed;
using System;

namespace IronText.MetadataCompiler
{
    class InlinedProductionCode : IActionCode
    {
        private readonly Fluent<EmitSyntax> emitCoder;
        private readonly Production         inlinedProd;
        private readonly ISemanticLoader    semanticLoader;
        private readonly VarsStack        localsStack;

        public InlinedProductionCode(
            Fluent<EmitSyntax> emitCoder,
            ISemanticLoader    semanticLoader,
            Production         inlinedProd,
            VarsStack          localsStack)
        {
            this.inlinedProd   = inlinedProd;
            this.semanticLoader = semanticLoader;
            this.emitCoder     = emitCoder;
            this.localsStack   = localsStack;
        }

        public IActionCode Emit(Pipe<EmitSyntax> emit)
        {
            emitCoder(emit);
            return this;
        }

        public IActionCode LdSemantic(string name)
        {
            if (!semanticLoader.LdSemantic(SemanticRef.ByName(name)))
            {
                throw new NotImplementedException("todo");
            }

            return this;
        }

        public IActionCode LdActionArgument(int index)
        {
            localsStack.LdSlot(localsStack.Count - inlinedProd.Components.Length + index);
            return this;
        }

        public IActionCode LdActionArgument(int index, Type argType)
        {
            LdActionArgument(index);
            if (argType.IsValueType)
            {
                emitCoder(il => il
                    .Unbox_Any(il.Types.Import(argType)));
            }

            return this;
        }

        public IActionCode LdMergerOldValue()
        {
            throw new System.NotImplementedException();
        }

        public IActionCode LdMergerNewValue()
        {
            throw new System.NotImplementedException();
        }

        public IActionCode LdMatcherTokenString()
        {
            throw new System.NotImplementedException();
        }

        public IActionCode ReturnFromAction()
        {
            throw new System.NotImplementedException();
        }

        public IActionCode SkipAction()
        {
            throw new System.NotImplementedException();
        }
    }
}
