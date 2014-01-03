using System;
using System.Linq;
using IronText.Extensibility;
using IronText.Extensibility.Bindings.Cil;
using IronText.Framework;
using IronText.Framework.Reflection;
using IronText.Lib.IL;
using IronText.Lib.Shared;

namespace IronText.MetadataCompiler
{
    class ScanActionCode : IScanActionCode
    {
        private Ref<Labels> RETURN;

        private EmitSyntax emit;
        private readonly Pipe<EmitSyntax> ldCursor;
        private readonly Ref<Types> declaringType;
        private readonly ScanCondition[] scanConditions;

        public ScanActionCode(
            EmitSyntax              emit, 
            IContextResolverCode    contextResolver,
            Pipe<EmitSyntax>        ldCursor,
            Ref<Types>              declaringType,
            ScanCondition[]         scanConditions)
        {
            this.emit            = emit;
            this.ldCursor        = ldCursor;
            this.ContextResolver = contextResolver;
            this.declaringType   = declaringType;
            this.scanConditions  = scanConditions;
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

            ScanCondition foundCondition = null;
            foreach (var condition in scanConditions)
            {
                var binding = condition.Bindings.OfType<CilScanConditionBinding>().Single();
                if (modeType.Equals(binding.ConditionType))
                {
                    foundCondition = condition;
                    break;
                }
            }

            if (foundCondition == null)
            {
                throw new InvalidOperationException("Internal Error: Mode not found.");
            }

            emit
                .Do(ldCursor)
                .LdMethodDelegate(declaringType, ScanModeMethods.GetMethodName(foundCondition.Index), typeof(Scan1Delegate))
                .Stfld((ScanCursor c) => c.CurrentMode)

                .Label(DONOT_CHANGE_MODE.Def)
                .Nop()
                ;

            return this;
        }
    }
}
