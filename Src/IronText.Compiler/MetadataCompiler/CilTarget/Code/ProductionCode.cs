using System;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection.Managed;
using IronText.Runtime;
using IronText.Reflection;

namespace IronText.MetadataCompiler
{
    class ProductionCode : IActionCode
    {
        private EmitSyntax                emit;
        private readonly Def<Labels>      returnLabel;
        private readonly Pipe<EmitSyntax> ldRuleArgs;
        private readonly Pipe<EmitSyntax> ldArgsStart;

        private ISemanticCode contextCode;

        public ProductionCode(
            EmitSyntax       emit,
            ISemanticCode    contextCode,
            Pipe<EmitSyntax> ldRuleArgs,
            Pipe<EmitSyntax> ldArgsStart,
            Def<Labels>      returnLabel)
        {
            this.emit        = emit;
            this.contextCode = contextCode;
            this.ldRuleArgs  = ldRuleArgs;
            this.ldArgsStart = ldArgsStart;
            this.returnLabel = returnLabel;
        }

        public IActionCode LdSemantic(string contextName)
        {
            contextCode.LdSemantic(SemanticRef.ByName(contextName));
            return this;
        }

        public IActionCode Emit(Pipe<EmitSyntax> pipe)
        {
            emit = pipe(emit);
            return this;
        }

        public IActionCode LdActionArgument(int index)
        {
            emit = emit
                .Do(ldRuleArgs)
                .Do(ldArgsStart);

            // Optimization for "+ 0".
            if (index != 0)
            {
                emit
                    .Ldc_I4(index)
                    .Add();
            }

            if (typeof(ActionNode).IsValueType)
            {
                emit = emit
                    .Ldelema(emit.Types.Import(typeof(ActionNode)));
            }
            else
            {
                emit = emit
                    .Ldelem_Ref();
            }

            emit = emit
                .Ldfld((ActionNode msg) => msg.Value)
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
            emit.Br(returnLabel.GetRef());
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
    }
}
