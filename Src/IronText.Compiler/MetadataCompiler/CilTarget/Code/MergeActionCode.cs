using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Reflection.Managed;

namespace IronText.MetadataCompiler
{
    public class MergeActionCode : IMergeActionCode
    {
        public Pipe<EmitSyntax> LoadOldValue;
        public Pipe<EmitSyntax> LoadNewValue;
        private EmitSyntax emit;

        public MergeActionCode(EmitSyntax emit, IContextResolverCode contextResolver)
        {
            this.emit = emit;
            ContextResolver = contextResolver;
        }

        public IContextResolverCode ContextResolver { get; private set; }

        public IMergeActionCode Emit(Pipe<EmitSyntax> pipe)
        {
            emit = emit.Do(pipe);
            return this;
        }

        public IMergeActionCode LdOldValue()
        {
            return Emit(LoadOldValue);
        }

        public IMergeActionCode LdNewValue()
        {
            return Emit(LoadNewValue);
        }
    }
}
