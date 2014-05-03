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
        private readonly ISemanticCode contextCode;
        private readonly Pipe<EmitSyntax> ldText;

        public MatcherCode(EmitSyntax emit, ISemanticCode contextCode, Pipe<EmitSyntax> ldText, Ref<Labels> RETURN)
        {
            this.emit            = emit;
            this.ldText        = ldText;
            this.contextCode     = contextCode;
            this.RETURN          = RETURN;
        }

        public IActionCode LdSemantic(string contextName)
        {
            contextCode.LdSemantic(contextName);
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

        public IActionCode LdActionArgument(int index)
        {
            throw new NotSupportedException();
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
