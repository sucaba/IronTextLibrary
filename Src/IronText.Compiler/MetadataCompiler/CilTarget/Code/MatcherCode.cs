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
        private readonly Pipe<EmitSyntax> ldCursor;
        private readonly Ref<Types> declaringType;

        public MatcherCode(
            EmitSyntax           emit, 
            IContextCode         contextCode,
            Pipe<EmitSyntax>     ldCursor,
            Ref<Types>           declaringType,
            Ref<Labels>          RETURN)
        {
            this.emit            = emit;
            this.ldCursor        = ldCursor;
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

        public IActionCode LdMatcherBuffer()
        {
            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Buffer);
            return this;
        }

        public IActionCode LdMatcherStartIndex()
        {
            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Start);
            return this;
        }

        public IActionCode LdMatcherLength()
        {
            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Marker)
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Start)
                .Sub()
                ;

            return this;
        }

        public IActionCode LdMatcherTokenString()
        {
            return this
                .LdMatcherBuffer()
                .LdMatcherStartIndex()
                .LdMatcherLength()
                .Emit(il => 
                    il.Newobj(() => new string(default(char[]), default(int), default(int))))
                ;
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
