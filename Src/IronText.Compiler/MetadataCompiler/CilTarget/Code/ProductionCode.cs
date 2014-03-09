﻿using System;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class ProductionCode : IProductionCode
    {
        public ProductionCode(EmitSyntax emit, IContextCode contextResolver)
        {
            this.emit = emit;
            ReturnLabel = emit.Labels.Generate();
            ContextResolver = contextResolver;
        }

        public Pipe<EmitSyntax> LdRule;
        public Pipe<EmitSyntax> LdRuleArgs;
        public Pipe<EmitSyntax> LdArgsStart;

        public Def<Labels>  ReturnLabel;
        private EmitSyntax emit;

        public IProductionCode Emit(Pipe<EmitSyntax> pipe)
        {
            emit = pipe(emit);
            return this;
        }

        public IContextCode ContextResolver { get; private set; }

        public IProductionCode LdRuleArg(int index)
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

        public IProductionCode LdRuleArg(int index, Type argType)
        {
            LdRuleArg(index);
            if (argType.IsValueType)
            {
                emit.Unbox_Any(emit.Types.Import(argType)); 
            }

            return this;
        }

        /// <summary>
        /// Takes single value from the .net stack and pushes it to the parser input stack
        /// </summary>
        public void PushRuleResult()
        {
        }

        public void EmitReturn()
        {
            emit.Br(ReturnLabel.GetRef());
        }
    }
}