using System.Linq;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class TermFactoryGenerator
    {
        private readonly Ref<Types> declaringType;
        private LanguageData        data;

        public TermFactoryGenerator(LanguageData data, Ref<Types> declaringType)
        {
            this.data = data;
            this.declaringType = declaringType;
        }

        public void Build(
            EmitSyntax          emit,
            Pipe<EmitSyntax>    ldCursor,
            Pipe<EmitSyntax>    ldTokenPtr)
        {
            var labels = emit.Labels;
            var locals = emit.Locals;

            var RETURN = labels.Generate();

            var valueTmp = locals.Generate();
            var tokenId  = locals.Generate();

            emit
                .Local(valueTmp, emit.Types.Object)
                .Ldnull()
                .Stloc(valueTmp.GetRef())

                .Local(tokenId, emit.Types.Int32)
                .Ldc_I4(-1)
                .Stloc(tokenId.GetRef())
                ;

            int ruleCount = data.Grammar.Conditions.Sum(cond => cond.Matchers.Count);

            var action = new Ref<Labels>[ruleCount];
            for (int i = 0; i != ruleCount; ++i)
            {
                action[i] = labels.Generate().GetRef();
            }

            emit
                .Do(ldCursor)
                .Ldfld((ScanCursor c) => c.CurrentActionId)
                .Switch(action)
                ;

            foreach (var cond in data.Grammar.Conditions)
            {
                var contextResolver = new ContextResolverCode(
                                        emit,
                                        il => il
                                            .Do(ldCursor)
                                            .Ldfld((ScanCursor c) => c.RootContext),
                                        null,
                                        data,
                                        cond.ContextProvider);
                var code = new MatcherActionCode(
                                emit,
                                contextResolver,
                                ldCursor,
                                declaringType,
                                data.Grammar.Conditions,
                                RETURN.GetRef());

                foreach (var matcher in cond.Matchers)
                {
                    emit
                        .Label(action[matcher.Index].Def)
                        .Ldc_I4(matcher.Outcome == null ? -1 : matcher.Outcome.Index)
                        .Stloc(tokenId.GetRef())
                        ;

                    var productionBinding = matcher.Joint.The<CilMatcher>();
                    productionBinding.Context.LoadForMatcher(code);
                    productionBinding.ActionBuilder(code);
                }
            }

            // Load null value for incorrectly implemented actions
            emit.Ldnull();

            emit
                .Label(RETURN)

                .Stloc(valueTmp.GetRef())
                .Do(ldTokenPtr)
                .Ldloc(valueTmp.GetRef())
                .Stind_Ref()

                .Ldloc(tokenId.GetRef())
                .Ret()
                ;
        }
    }
}
