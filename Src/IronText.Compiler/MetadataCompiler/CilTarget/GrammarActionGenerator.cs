using System.Diagnostics;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Runtime;
using IronText.Reflection.Managed;

namespace IronText.MetadataCompiler
{
    internal class GrammarActionGenerator
    {
        public GrammarActionGenerator()
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
            var contextResolverCode = new ContextResolverCode(
                                            emit,
                                            il => il.Ldarg(ctx),
                                            il => il.Ldarg(lookbackStart),
                                            data.DefinitionType,
                                            data.LocalParseContexts);

            var code = new GrammarActionCode(emit, contextResolverCode)
            {
                LdRule       = il => il.Ldarg(ruleId),
                LdRuleArgs   = il => il.Ldarg(ruleArgs),
                LdArgsStart  = il => il.Ldarg(argsStart)
            };

            var defaultLabel = emit.Labels.Generate();
            var endWithSingleResultLabel = emit.Labels.Generate();
            var jumpTable = new Ref<Labels>[data.Grammar.Productions.Count];
            for (int i = 0; i != jumpTable.Length; ++i)
            {
                jumpTable[i] = emit.Labels.Generate().GetRef();
            }

            emit
                .Do(code.LdRule)
                .Switch(jumpTable)
                .Br(defaultLabel.GetRef());

            foreach (var prod in data.Grammar.Productions)
            {
                Debug.Assert(prod != null);

                emit.Label(jumpTable[prod.Index].Def);
                // TODO: Support for CompositeProductionAction
                var action = (SimpleProductionAction)prod.Action;
                if (action != null && action.Joint.Has<CilProduction>())
                {
                    bool first = true;
                    foreach (var binding in action.Joint.All<CilProduction>())
                    {
                        if (binding != null)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                // Result of this rule supersedes result of the prvious one
                                code.Emit(il => il.Pop());
                            }

                            binding.ActionBuilder(code);
                        }
                    }
                }
                else
                {
                    // Augumented start rule has null action and should never be invoked.
                    // Also it is possible that for some platforms production may have default
                    // action.
                    emit.Ldnull();
                }

                emit.Br(endWithSingleResultLabel.GetRef());
            }

            emit
                .Label(defaultLabel)
                .Ldnull()
                .Label(endWithSingleResultLabel);
            code.PushRuleResult();
            emit
                .Label(code.ReturnLabel)
                .Ret();
        }
    }
}
