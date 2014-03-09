using System;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class MatcherCode : IMatcherCode
    {
        private Ref<Labels> RETURN;

        private EmitSyntax emit;
        private readonly Pipe<EmitSyntax> ldCursor;
        private readonly Ref<Types> declaringType;
        private readonly ConditionCollection conditions;

        public MatcherCode(
            EmitSyntax           emit, 
            IContextCode         contextCode,
            Pipe<EmitSyntax>     ldCursor,
            Ref<Types>           declaringType,
            ConditionCollection  conditions,
            Ref<Labels>          RETURN)
        {
            this.emit            = emit;
            this.ldCursor        = ldCursor;
            this.ContextCode     = contextCode;
            this.declaringType   = declaringType;
            this.conditions      = conditions;
            this.RETURN          = RETURN;
        }

        public IContextCode ContextCode { get; private set; }

        public IMatcherCode ReturnFromAction()
        {
            emit.Br(RETURN);
            return this;
        }

        public IMatcherCode SkipAction()
        {
            emit
                .Ldnull()
                .Br(RETURN);
            return this;
        }

        public IMatcherCode Emit(Pipe<EmitSyntax> emitPipe)
        {
            emit = emitPipe(emit);
            return this;
        }

        public IMatcherCode LdBuffer()
        {
            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Buffer);
            return this;
        }

        public IMatcherCode LdStartIndex()
        {
            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Start);
            return this;
        }

        public IMatcherCode LdLength()
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

        public IMatcherCode LdTokenString()
        {
            return this
                .LdBuffer()
                .LdStartIndex()
                .LdLength()
                .Emit(il => 
                    il.Newobj(() => new string(default(char[]), default(int), default(int))))
                ;
        }

        public IMatcherCode ChangeCondition(Type conditionType)
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

            var condition = FindConditionByType(conditionType);
            if (condition == null)
            {
                throw new InvalidOperationException("Internal Error: Condition not found.");
            }

            emit
                .Do(ldCursor)
                .LdMethodDelegate(declaringType, ConditionMethods.GetMethodName(condition.Index), typeof(Scan1Delegate))
                .Stfld((ScanCursor c) => c.CurrentMode)

                .Label(DONOT_CHANGE_MODE.Def)
                .Nop()
                ;

            return this;
        }

        private Condition FindConditionByType(Type conditionType)
        {
            Condition result = null;
            foreach (var condition in this.conditions)
            {
                var binding = condition.Joint.The<CilCondition>();
                if (conditionType.Equals(binding.ConditionType))
                {
                    result = condition;
                    break;
                }
            }

            return result;
        }
    }
}
