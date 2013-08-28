using System;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Framework;

namespace IronText.MetadataCompiler
{
    class ScanActionCode : IScanActionCode
    {
        private Ref<Labels> RETURN;

        private EmitSyntax emit;
        private readonly Pipe<EmitSyntax> ldCursor;
        private readonly Ref<Types> declaringType;
        private readonly ScanMode[] scanModes;

        public ScanActionCode(
            EmitSyntax              emit, 
            IContextResolverCode    contextResolver,
            Pipe<EmitSyntax>      ldCursor,
            Ref<Types>                 declaringType,
            ScanMode[]              scanModes)
        {
            this.emit            = emit;
            this.ldCursor        = ldCursor;
            this.ContextResolver = contextResolver;
            this.declaringType   = declaringType;
            this.scanModes       = scanModes;
        }

        public IContextResolverCode ContextResolver { get; private set; }

        public void Init(EmitSyntax emit, Ref<Labels> RETURN)
        {
            this.RETURN = RETURN;
        }

        public IScanActionCode ReturnFromAction()
        {
            emit.Br(RETURN);
            return this;
        }

        public IScanActionCode SkipAction()
        {
            emit
                .Ldnull()
                .Br(RETURN);
            return this;
        }

        public IScanActionCode Emit(Pipe<EmitSyntax> emitPipe)
        {
            emit = emitPipe(emit);
            return this;
        }

        public IScanActionCode LdBuffer()
        {
            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Buffer);
            return this;
        }

        public IScanActionCode LdStartIndex()
        {
            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Start);
            return this;
        }

        public IScanActionCode LdLength()
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

        public IScanActionCode LdTokenString()
        {
            return this
                .LdBuffer()
                .LdStartIndex()
                .LdLength()
                .Emit(il => 
                    il.Newobj(() => new string(default(char[]), default(int), default(int))))
                ;
        }

        public IScanActionCode ChangeMode(Type modeType)
        {
            var contextTemp = emit.Locals.Generate().GetRef();
            var DONOT_CHANGE_MODE = emit.Labels.Generate().GetRef();
            emit
                .Local(contextTemp.Def, emit.Types.Object)
                .Stloc(contextTemp)
                .Ldloc(contextTemp)
                .Brfalse(DONOT_CHANGE_MODE)
                .Do(ldCursor)
                .Ldloc(contextTemp)
                .Stfld((ScanCursor c) => c.RootContext)
                ;

            ScanMode scanMode = null;
            int modeIndex = 0;
            foreach (var mode in scanModes)
            {
                if (modeType.Equals(mode.ScanModeType))
                {
                    scanMode = mode;
                    break;
                }

                ++modeIndex;
            }

            if (modeIndex == scanModes.Length)
            {
                throw new InvalidOperationException("Internal Error: Mode not found.");
            }

            emit
                .Do(ldCursor)
                .LdMethodDelegate(declaringType, ScanModeMethods.GetMethodName(modeIndex), typeof(Scan1Delegate))
                .Stfld((ScanCursor c) => c.CurrentMode)

                .Label(DONOT_CHANGE_MODE.Def)
                .Nop()
                ;

            return this;
        }
    }
}
