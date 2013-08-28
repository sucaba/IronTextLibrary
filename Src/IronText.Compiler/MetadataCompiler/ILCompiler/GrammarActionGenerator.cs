using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Framework;

namespace IronText.MetadataCompiler
{
    public class GrammarActionGenerator
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

            Def<Args> rule          =  args.Args.Generate("rule");
            Def<Args> ruleArgs      =  args.Args.Generate("ruleArgs");
            Def<Args> argsStart     =  args.Args.Generate("argsStart");
            Def<Args> ctx           =  args.Args.Generate("rootContext");
            Def<Args> stackLookback =  args.Args.Generate("startLookback");

            var emit = args
                    .Argument(context.Types.Import(typeof(BnfRule)), rule)
                    .Argument(context.Types.Import(typeof(Msg[])), ruleArgs)
                    .Argument(context.Types.Int32, argsStart)
                    .Argument(context.Types.Object, ctx)
                    .Argument(context.Types.Import(typeof(IStackLookback<Msg>)), stackLookback)
                    .EndArgs()

                .BeginBody();

            rule.Name          = "rule";
            ruleArgs.Name      = "args";
            argsStart.Name     = "argsStart";
            ctx.Name           = "ctx";
            stackLookback.Name = "stackLookback";

            BuildBody(
                emit, 
                data, 
                rule.GetRef(),
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
                rule:       args[0],
                ruleArgs:   args[1],
                argsStart:  args[2],
                ctx:        args[3],
                lookbackStart: args[4]);
        }

        public void BuildBody(
            EmitSyntax emit, 
            LanguageData data,
            Ref<Args> rule,
            Ref<Args> ruleArgs,
            Ref<Args> argsStart,
            Ref<Args> ctx,
            Ref<Args> lookbackStart)
        {
            var contextResolverCode = new ContextResolverCode(
                                            emit,
                                            il => il.Ldarg(ctx),
                                            il => il.Ldarg(lookbackStart),
                                            data.RootContextType,
                                            data.LocalParseContexts);

            var code = new GrammarActionCode(emit, contextResolverCode)
            {
                LdRule       = il => il.Ldarg(rule),
                LdRuleArgs   = il => il.Ldarg(ruleArgs),
                LdArgsStart  = il => il.Ldarg(argsStart)
            };

            var defaultLabel = emit.Labels.Generate();
            var endWithSingleResultLabel = emit.Labels.Generate();
            var jumpTable = new Ref<Labels>[data.RuleActionBuilders.Length];
            for (int i = 0; i != jumpTable.Length; ++i)
            {
                jumpTable[i] = emit.Labels.Generate().GetRef();
            }

            emit
                .Do(code.LdRule)
                .Ldfld((BnfRule r) => r.Id)
                .Switch(jumpTable)
                .Br(defaultLabel.GetRef());

            for (int i = 0; i != data.RuleActionBuilders.Length; ++i)
            {
                emit.Label(jumpTable[i].Def);
                if (data.RuleActionBuilders[i] != null)
                {
                    bool first = true;
                    foreach (var builder in data.RuleActionBuilders[i])
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

                        builder(code);
                    }
                }
                else
                {
                    // Augumented start rule has null action and should never be invoked.
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
