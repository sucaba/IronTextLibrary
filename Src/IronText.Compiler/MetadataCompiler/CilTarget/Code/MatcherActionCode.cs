using System;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class MatcherActionCode : IMatcherActionCode
    {
        private Ref<Labels> RETURN;

        private EmitSyntax emit;
        private readonly Pipe<EmitSyntax> ldCursor;
        private readonly Ref<Types> declaringType;
        private readonly ConditionCollection conditions;

        public MatcherActionCode(
            EmitSyntax              emit, 
            IContextResolverCode    contextResolver,
            Pipe<EmitSyntax>        ldCursor,
            Ref<Types>              declaringType,
            ConditionCollection     conditions,
            Ref<Labels>             RETURN)
        {
            this.emit            = emit;
            this.ldCursor        = ldCursor;
            this.ContextResolver = contextResolver;
            this.declaringType   = declaringType;
            this.conditions      = conditions;
            this.RETURN          = RETURN;
        }

        public IContextResolverCode ContextResolver { get; private set; }

        public IMatcherActionCode ReturnFromAction()
        {
            emit.Br(RETURN);
            return this;
        }

        public IMatcherActionCode SkipAction()
        {
            emit
                .Ldnull()
                .Br(RETURN);
            return this;
        }

        public IMatcherActionCode Emit(Pipe<EmitSyntax> emitPipe)
        {
            emit = emitPipe(emit);
            return this;
        }

        public IMatcherActionCode LdBuffer()
        {
            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Buffer);
            return this;
        }

        public IMatcherActionCode LdStartIndex()
        {
            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.Start);
            return this;
        }

        public IMatcherActionCode LdLength()
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

        public IMatcherActionCode LdTokenString()
        {
            return this
                .LdBuffer()
                .LdStartIndex()
                .LdLength()
                .Emit(il => 
                    il.Newobj(() => new string(default(char[]), default(int), default(int))))
                ;
        }

        public IMatcherActionCode ChangeMode(Type conditionType)
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

        private Condition FindConditionByType(Type modeType)
        {
            Condition result = null;
            foreach (var condition in this.conditions)
            {
                var binding = condition.Joint.The<CilCondition>();
                if (modeType.Equals(binding.ConditionType))
                {
                    result = condition;
                    break;
                }
            }

            return result;
        }
    }
}
