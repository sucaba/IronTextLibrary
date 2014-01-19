using System.Linq;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Extensibility.Cil;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.IL.Generators;
using IronText.Lib.Shared;

namespace IronText.MetadataCompiler
{
    internal class MergeActionGenerator
    {
        public MergeActionGenerator()
        {
        }

        public ClassSyntax BuildMethod(ClassSyntax context, string methodName, LanguageData data)
        {
            var args = context.Method()
                            .Static
                            .Returning(context.Types.Object)
                            .Named(methodName)
                            .BeginArgs();

            Def<Args> tokenId       =  args.Args.Generate("token");
            Def<Args> oldValue      =  args.Args.Generate("oldValue");
            Def<Args> newValue      =  args.Args.Generate("newValue");
            Def<Args> ctx           =  args.Args.Generate("rootContext");
            Def<Args> stackLookback =  args.Args.Generate("startLookback");

            var emit = args
                    .Argument(context.Types.Int32, tokenId)
                    .Argument(context.Types.Object, oldValue)
                    .Argument(context.Types.Object, newValue)
                    .Argument(context.Types.Object, ctx)
                    .Argument(context.Types.Import(typeof(IStackLookback<Msg>)), stackLookback)
                    .EndArgs()

                .BeginBody();

            BuildBody(
                emit, 
                data, 
                tokenId.GetRef(),
                oldValue.GetRef(),
                newValue.GetRef(),
                ctx.GetRef(),
                stackLookback.GetRef());

            return emit.EndBody();
        }

        public void BuildBody(EmitSyntax emit, LanguageData data, Ref<Args>[] args)
        {
            BuildBody(
                emit,
                data,
                tokenId:    args[0],
                oldValue:   args[1],
                newValue:   args[2],
                ctx:        args[3],
                lookbackStart: args[4]);
        }

        public void BuildBody(
            EmitSyntax emit, 
            LanguageData data,
            Ref<Args> tokenId,
            Ref<Args> oldValue,
            Ref<Args> newValue,
            Ref<Args> ctx,
            Ref<Args> lookbackStart)
        {
            var mergers = data.Grammar.Mergers;

            if (mergers.Count == 0)
            {
                emit
                    .Ldarg(oldValue)
                    .Ret();

                return;
            }

            var contextResolverCode = new ContextResolverCode(
                                            emit,
                                            il => il.Ldarg(ctx),
                                            il => il.Ldarg(lookbackStart),
                                            data.RootContextType,
                                            data.LocalParseContexts);

            var code = new MergeActionCode(emit, contextResolverCode)
            {
                LoadOldValue   = il => il.Ldarg(oldValue),
                LoadNewValue   = il => il.Ldarg(newValue),
            };

            var tokenToRuleIndex = new MutableIntMap<int>(
                                        mergers.Select(
                                            merger => 
                                                new IntArrow<int>(merger.Symbol.Index, merger.Index)));
            tokenToRuleIndex.DefaultValue = -1;

            var ids = mergers.Select(m => m.Symbol.Index);
            IntInterval possibleBounds = new IntInterval(ids.Min(), ids.Max());
            var switchGenerator = SwitchGenerator.Sparse(tokenToRuleIndex, possibleBounds);
            switchGenerator.Build(
                emit,
                il => il.Ldarg(tokenId),
                (il, value) =>
                {
                    if (value < 0)
                    {
                        emit
                            .Ldarg(newValue)
                            .Ret();
                    }
                    else
                    {
                        var merger = mergers[value];
                        merger.Joint.The<CilMergerDef>().ActionBuilder(code);
                        emit.Ret();
                    }
                });

            emit
                .Ret();
        }
    }
}
