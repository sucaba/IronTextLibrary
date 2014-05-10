using System.Diagnostics;
using System.Linq;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;
using IronText.MetadataCompiler.CilTarget;
using IronText.Framework;

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
                    .Argument(context.Types.Import(typeof(ActionNode[])), ruleArgs)
                    .Argument(context.Types.Int32, argsStart)
                    .Argument(context.Types.Object, ctx)
                    .Argument(context.Types.Import(typeof(IStackLookback<ActionNode>)), stackLookback)
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

            var globalSemanticCode = new GlobalSemanticLoader(emit, il => il.Ldarg(ctx), data.Grammar.Globals);

            var localSemanticCode = new SemanticLoader(
                globalSemanticCode,
                emit,
                il => il.Ldarg(lookbackStart),
                data,
                data.SemanticBindings);

            var code = Fluent.Create<IActionCode>(new ProductionCode(
                emit, 
                localSemanticCode,
                ldRuleArgs:  il => il.Ldarg(ruleArgs),
                ldArgsStart: il => il.Ldarg(argsStart),
                returnLabel: returnLabel));

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

                CompileProduction(code, prod);

                emit.Br(endWithSingleResultLabel.GetRef());
            }

            emit
                .Label(defaultLabel)
                .Ldnull()
                .Label(endWithSingleResultLabel)
                .Label(returnLabel)
                .Ret();
        }

        public static void CompileProduction(Fluent<IActionCode> code, Production prod)
        {
            var compiler = new ProductionCompiler(code);
#if false
            for (int i = 0; i != prod.Size; ++i)
            {
                code = code.LdActionArgument(i);
            }
#endif

            compiler.Execute(prod);
        }
    }
}
