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
            Pipe<EmitSyntax>    ldRootContext,
            Pipe<EmitSyntax>    ldAction,
            Pipe<EmitSyntax>    ldText
//            Pipe<EmitSyntax>    ldCursor,
//            Pipe<EmitSyntax>    ldTokenPtr,
            )
        {
            var labels = emit.Labels;
            var locals = emit.Locals;

            var RETURN = labels.Generate();

            int ruleCount = data.Grammar.Matchers.Count;

            var actionLabels = new Ref<Labels>[ruleCount];
            for (int i = 0; i != ruleCount; ++i)
            {
                actionLabels[i] = labels.Generate().GetRef();
            }

            emit
                .Do(ldAction)
                .Switch(actionLabels)
                ;

            var contextResolver = new ContextCode(
                                    emit,
                                    il => il.Do(ldRootContext),
                                    null,
                                    data,
                                    data.Grammar.GlobalContextProvider);
            IActionCode code = new MatcherCode(
                            emit,
                            contextResolver,
                            ldText,
                            declaringType,
                            RETURN.GetRef());

            foreach (var matcher in data.Grammar.Matchers)
            {
                emit.Label(actionLabels[matcher.Index].Def);

                var binding = matcher.Joint.The<CilMatcher>();
                code = code
                    .Do(binding.Context.Load)
                    .Do(binding.ActionBuilder);
            }

            // Load null value for incorrectly implemented actions
            emit.Ldnull();

            emit
                .Label(RETURN)
                .Ret()
                ;
        }
    }
}
