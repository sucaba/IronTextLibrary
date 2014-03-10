using System;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class ProductionCode : IActionCode
    {
        private EmitSyntax      emit;
        public Def<Labels>      ReturnLabel;
        private readonly Pipe<EmitSyntax> LdRuleArgs;
        private readonly Pipe<EmitSyntax> LdArgsStart;

        private IContextCode contextCode;

        public ProductionCode(
            EmitSyntax       emit,
            IContextCode     contextCode,
            Pipe<EmitSyntax> ldRuleArgs,
            Pipe<EmitSyntax> ldArgsStart,
            Def<Labels>      returnLabel)
        {
            this.emit        = emit;
            this.contextCode = contextCode;
            this.LdRuleArgs  = ldRuleArgs;
            this.LdArgsStart = ldArgsStart;
            this.ReturnLabel = returnLabel;
        }

        public IActionCode LdContext(string contextName)
        {
            contextCode.LdContext(contextName);
            return this;
        }

        public IActionCode Emit(Pipe<EmitSyntax> pipe)
        {
            emit = pipe(emit);
            return this;
        }

        private IActionCode LdActionArgument(int index)
        {
            emit = emit
                .Do(LdRuleArgs)
                .Do(LdArgsStart);

            // Optmization for "+ 0".
            if (index != 0)
            {
                emit
                    .Ldc_I4(index)
                    .Add();
            }

            if (typeof(Msg).IsValueType)
            {
                emit = emit
                    .Ldelema(emit.Types.Import(typeof(Msg)));
            }
            else
            {
                emit = emit
                    .Ldelem_Ref();
            }

            emit = emit
                .Ldfld((Msg msg) => msg.Value)
                ;

            return this;
        }

        public IActionCode LdActionArgument(int index, Type argType)
        {
            LdActionArgument(index);
            if (argType.IsValueType)
            {
                emit.Unbox_Any(emit.Types.Import(argType)); 
            }

            return this;
        }

        public void EmitReturn()
        {
            emit.Br(ReturnLabel.GetRef());
        }

        public IActionCode LdMergerOldValue()
        {
            throw new NotSupportedException();
        }

        public IActionCode LdMergerNewValue()
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
