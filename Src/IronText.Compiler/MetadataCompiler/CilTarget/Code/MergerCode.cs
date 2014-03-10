using System;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Reflection.Managed;

namespace IronText.MetadataCompiler
{
    class MergeCode : IActionCode
    {
        public Pipe<EmitSyntax> LoadOldValue;
        public Pipe<EmitSyntax> LoadNewValue;
        private EmitSyntax emit;
        private readonly IContextCode contextCode;

        public MergeCode(EmitSyntax emit, IContextCode contextCode)
        {
            this.emit        = emit;
            this.contextCode = contextCode;
        }

        public IActionCode LdContext(string contextName)
        {
            contextCode.LdContext(contextName);
            return this;
        }

        public IActionCode Emit(Pipe<EmitSyntax> pipe)
        {
            emit = emit.Do(pipe);
            return this;
        }

        public IActionCode LdMergerOldValue()
        {
            return Emit(LoadOldValue);
        }

        public IActionCode LdMergerNewValue()
        {
            return Emit(LoadNewValue);
        }

        public IActionCode LdActionArgument(int index)
        {
            throw new NotSupportedException();
        }

        public IActionCode LdActionArgument(int index, Type argType)
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

        public IActionCode ChangeCondition(Type conditionType)
        {
            throw new NotSupportedException();
        }
    }
}
