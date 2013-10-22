using System;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;

namespace IronText.MetadataCompiler
{
    class TermFactoryGenerator
    {
        private readonly Ref<Types>    declaringType;
        private readonly ScanMode[] scanModes;
        private readonly ITokenRefResolver tokenRefResolver;

        public TermFactoryGenerator(ScanMode[] scanModes, Ref<Types> declaringType, ITokenRefResolver tokenRefResolver)
        {
            this.scanModes = scanModes;
            this.declaringType = declaringType;
            this.tokenRefResolver = tokenRefResolver;
        }

        public void Build(
            EmitSyntax          emit,
            ContextResolverCode contextResolver,
            Pipe<EmitSyntax>  ldCursor,
            Pipe<EmitSyntax>  ldTokenPtr)
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

            int ruleCount = scanModes.Sum(mode => mode.ScanRules.Count);

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

            var actionContext = new ScanActionCode(emit, contextResolver, ldCursor, declaringType, scanModes);
            actionContext.Init(emit, RETURN.GetRef());

            int j = 0;
            foreach (var mode in scanModes)
            {
                // Each mode has its own root context type:
                contextResolver.RootContextType = mode.ScanModeType;
                foreach (var rule in mode.ScanRules)
                {
                    emit
                        .Label(action[j].Def)
                        .Ldc_I4(GetRuleResultId(rule))
                        .Stloc(tokenId.GetRef())
                        ;
                    rule.ActionBuilder(actionContext);

                    ++j;
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

        private int GetRuleResultId(IScanRule rule)
        {
            if (rule is ISkipScanRule)
            {
                return -1;
            }

            var asSingleTokenRule = rule as ISingleTokenScanRule;
            if (asSingleTokenRule == null)
            {
                throw new NotSupportedException("Internal error: multitoken rules are not supported.");
            }

            return tokenRefResolver.GetId(asSingleTokenRule.AnyTokenRef);
        }
    }
}
