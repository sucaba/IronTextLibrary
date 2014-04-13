using System;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class MatcherCode : IActionCode
    {
        private Ref<Labels> RETURN;

        private EmitSyntax emit;
        private readonly IContextCode contextCode;
        private readonly Pipe<EmitSyntax> ldText;
        private readonly Ref<Types> declaringType;

        public MatcherCode(
            EmitSyntax           emit, 
            IContextCode         contextCode,
            Pipe<EmitSyntax>     ldText,
            Ref<Types>           declaringType,
            Ref<Labels>          RETURN)
        {
            this.emit            = emit;
            this.ldText        = ldText;
            this.contextCode     = contextCode;
            this.declaringType   = declaringType;
            this.RETURN          = RETURN;
        }

        public IActionCode LdContext(string contextName)
        {
            contextCode.LdContext(contextName);
            return this;
        }

        public IActionCode ReturnFromAction()
        {
            emit.Br(RETURN);
            return this;
        }

        public IActionCode SkipAction()
        {
            emit
                .Ldnull()
                .Br(RETURN);
            return this;
        }

        public IActionCode Emit(Pipe<EmitSyntax> emitPipe)
        {
            emit = emitPipe(emit);
            return this;
        }

        public IActionCode LdMatcherTokenString()
        {
            return this.Emit(ldText);
        }

        public IActionCode LdActionArgument(int index, Type argType)
        {
            throw new NotSupportedException();
        }

        public IActionCode LdMergerOldValue()
        {
            throw new NotSupportedException();
        }

        public IActionCode LdMergerNewValue()
        {
            throw new NotSupportedException();
        }
    }
}
