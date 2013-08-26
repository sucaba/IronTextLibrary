using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.MetadataCompiler
{
    class SwitchActionCode : ISwitchActionCode
    {
        private EmitSyntax emit;
        private Pipe<EmitSyntax> ldExitReceiver;
        private Pipe<EmitSyntax> ldLanguage;

        public SwitchActionCode(
            IContextResolverCode contextResolver,
            EmitSyntax           emit,
            Pipe<EmitSyntax>   ldExitReceiver,
            Pipe<EmitSyntax>   ldLanguage)
        {
            ContextResolver = contextResolver;
            this.emit = emit;
            this.ldExitReceiver = ldExitReceiver;
            this.ldLanguage = ldLanguage;
        }

        public IContextResolverCode ContextResolver { get; private set; }

        public ISwitchActionCode Emit(Pipe<EmitSyntax> emitPipe)
        {
            emit = emitPipe(emit);
            return this;
        }

        public ISwitchActionCode LdLanguage()
        {
            emit = emit.Do(this.ldLanguage);
            return this;
        }

        public ISwitchActionCode LdExitReceiver()
        {
            emit = emit.Do(ldExitReceiver);
            return this;
        }
    }
}
