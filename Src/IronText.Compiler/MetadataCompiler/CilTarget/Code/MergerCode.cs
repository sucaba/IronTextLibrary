using IronText.Framework;
using IronText.Lib.IL;
using IronText.Reflection.Managed;

namespace IronText.MetadataCompiler
{
    class MergeCode : IMergerCode
    {
        public Pipe<EmitSyntax> LoadOldValue;
        public Pipe<EmitSyntax> LoadNewValue;
        private EmitSyntax emit;

        public MergeCode(EmitSyntax emit, IContextCode contextResolver)
        {
            this.emit = emit;
            ContextResolver = contextResolver;
        }

        public IContextCode ContextResolver { get; private set; }

        public IMergerCode Emit(Pipe<EmitSyntax> pipe)
        {
            emit = emit.Do(pipe);
            return this;
        }

        public IMergerCode LdOldValue()
        {
            return Emit(LoadOldValue);
        }

        public IMergerCode LdNewValue()
        {
            return Emit(LoadNewValue);
        }
    }
}
