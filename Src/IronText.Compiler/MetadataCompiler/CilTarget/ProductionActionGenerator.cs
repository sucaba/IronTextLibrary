﻿using System.Diagnostics;
using System.Linq;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    internal class ProductionActionGenerator
    {
        public ProductionActionGenerator()
        {
        }

        public ClassSyntax BuildMethod(ClassSyntax context, string methodName, LanguageData data)
        {
            var args = context.Method()
                            .Static
                            .Returning(context.Types.Object)
                            .Named(methodName)
                            .BeginArgs();

            Def<Args> ruleId        =  args.Args.Generate("ruleId");
            Def<Args> ruleArgs      =  args.Args.Generate("ruleArgs");
            Def<Args> argsStart     =  args.Args.Generate("argsStart");
            Def<Args> ctx           =  args.Args.Generate("rootContext");
            Def<Args> stackLookback =  args.Args.Generate("startLookback");

            var emit = args
                    .Argument(context.Types.Int32, ruleId)
                    .Argument(context.Types.Import(typeof(Msg[])), ruleArgs)
                    .Argument(context.Types.Int32, argsStart)
                    .Argument(context.Types.Object, ctx)
                    .Argument(context.Types.Import(typeof(IStackLookback<Msg>)), stackLookback)
                    .EndArgs()

                .BeginBody();

            ruleId.Name        = "ruleId";
            ruleArgs.Name      = "args";
            argsStart.Name     = "argsStart";
            ctx.Name           = "ctx";
            stackLookback.Name = "stackLookback";

            BuildBody(
                emit, 
                data, 
                ruleId.GetRef(),
                ruleArgs.GetRef(),
                argsStart.GetRef(),
                ctx.GetRef(),
                stackLookback.GetRef());

            return emit.EndBody();
        }

        public void BuildBody(EmitSyntax emit, LanguageData data, Ref<Args>[] args)
        {
            BuildBody(
                emit,
                data,
                ruleId:     args[0],
                ruleArgs:   args[1],
                argsStart:  args[2],
                ctx:        args[3],
                lookbackStart: args[4]);
        }

        public void BuildBody(
            EmitSyntax emit, 
            LanguageData data,
            Ref<Args> ruleId,
            Ref<Args> ruleArgs,
            Ref<Args> argsStart,
            Ref<Args> ctx,
            Ref<Args> lookbackStart)
        {
            Def<Labels> returnLabel = emit.Labels.Generate();

            var contextCode = new ContextCode(
                emit,
                il => il.Ldarg(ctx),
                il => il.Ldarg(lookbackStart),
                data,
                data.Grammar.GlobalContextProvider,
                data.LocalParseContexts);

            IActionCode code = new ProductionCode(
                emit, 
                contextCode,
                ldRuleArgs:  il => il.Ldarg(ruleArgs),
                ldArgsStart: il => il.Ldarg(argsStart),
                returnLabel: returnLabel);

            var defaultLabel = emit.Labels.Generate();
            var endWithSingleResultLabel = emit.Labels.Generate();
            var jumpTable = new Ref<Labels>[data.Grammar.Productions.Count];
            for (int i = 0; i != jumpTable.Length; ++i)
            {
                jumpTable[i] = emit.Labels.Generate().GetRef();
            }

            emit
                .Do(il => il.Ldarg(ruleId))
                .Switch(jumpTable)
                .Br(defaultLabel.GetRef());

            foreach (var prod in data.Grammar.Productions)
            {
                emit.Label(jumpTable[prod.Index].Def);

                if (0 == prod.Actions.Count)
                {
                    // Augumented start rule has null action and should never be invoked.
                    // Also it is possible that for some platforms production may have default
                    // action.
                    emit.Ldnull();
                }
                else
                {
                    foreach (var action in prod.Actions)
                    {
                        code = GenerateActionCode(code, action);
                    }
                }

                emit.Br(endWithSingleResultLabel.GetRef());
            }

            emit
                .Label(defaultLabel)
                .Ldnull()
                .Label(endWithSingleResultLabel)
                .Label(returnLabel)
                .Ret();
        }

        private static IActionCode GenerateActionCode(IActionCode code, ForeignAction action)
        {
            bool first = true;
            foreach (var binding in action.Joint.All<CilProduction>())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    // Result of this rule supersedes result of the prvious one
                    code = code.Emit(il => il.Pop());
                }

                code = code
                    .Do(binding.Context.Load)
                    .Do(binding.ActionBuilder)
                    ;
            }

            return code;
        }
    }
}
