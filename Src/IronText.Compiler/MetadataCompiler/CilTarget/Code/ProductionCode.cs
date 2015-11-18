using System;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection.Managed;
using IronText.Runtime;
using IronText.Reflection;
using IronText.Compilation;

namespace IronText.MetadataCompiler
{
    class ProductionCode : IActionCode
    {
        private Fluent<EmitSyntax> emitCoder;
        private ISemanticLoader    semanticLoader;
        private readonly VarsStack varsStack;
        private readonly int       varsStackStart;

        public ProductionCode(
            Fluent<EmitSyntax>  emitCoder,
            ISemanticLoader     semanticLoader,
            VarsStack           varsStack,
            int                 varsStackStart)
        {
            this.emitCoder      = emitCoder;
            this.semanticLoader = semanticLoader;
            this.varsStack      = varsStack;
            this.varsStackStart = varsStackStart;
        }

        public IActionCode LdSemantic(string name)
        {
            if (!semanticLoader.LdSemantic(SemanticRef.ByName(name)))
            {
                var msg = string.Format("Undefined semantic value for reference '{0}'", name);
                throw new InvalidOperationException(msg);
            }

            return this;
        }

        public IActionCode Emit(Pipe<EmitSyntax> pipe)
        {
            emitCoder(pipe);
            return this;
        }

        public IActionCode LdActionArgument(int index)
        {
            varsStack.LdSlot(varsStackStart + index);
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
            throw new NotSupportedException();
        }

        public IActionCode LdMergerNewValue()
        {
            throw new NotSupportedException();
        }

        public IActionCode LdMatcherTokenString()
        {
            throw new NotSupportedException();
        }

        public IActionCode LdMatcherBuffer()
        {
            throw new NotSupportedException();
        }

        public IActionCode LdMatcherStartIndex()
        {
            throw new NotSupportedException();
        }

        public IActionCode LdMatcherLength()
        {
            throw new NotSupportedException();
        }

        public IActionCode ReturnFromAction()
        {
            throw new NotSupportedException();
        }

        public IActionCode SkipAction()
        {
            throw new NotSupportedException();
        }
    }
}
