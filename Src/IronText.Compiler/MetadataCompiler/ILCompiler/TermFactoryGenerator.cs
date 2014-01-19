using System;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Framework.Reflection;
using IronText.Lib.IL;
using IronText.Lib.Shared;

namespace IronText.MetadataCompiler
{
    class TermFactoryGenerator
    {
        private readonly Ref<Types>        declaringType;
        private readonly ScanCondition[]   scanConditions;
        private readonly ITokenRefResolver tokenRefResolver;

        public TermFactoryGenerator(ScanConditionCollection scanModes, Ref<Types> declaringType, ITokenRefResolver tokenRefResolver)
        {
            this.scanConditions        = scanModes.ToArray();
            this.declaringType    = declaringType;
            this.tokenRefResolver = tokenRefResolver;
        }

        public void Build(
            EmitSyntax          emit,
            ContextResolverCode contextResolver,
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

            int ruleCount = scanConditions.Sum(mode => mode.ScanProductions.Count);

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

            var actionContext = new ScanActionCode(emit, contextResolver, ldCursor, declaringType, scanConditions);
            actionContext.Init(emit, RETURN.GetRef());

            foreach (var condition in scanConditions)
            {
                var conditionBinding = condition.Joint.The<CilScanConditionDef>();

                // Each mode has its own root context type:
                contextResolver.RootContextType = conditionBinding.ConditionType;
                foreach (var scanProduction in condition.ScanProductions)
                {
                    emit
                        .Label(action[scanProduction.Index].Def)
                        .Ldc_I4(scanProduction.Outcome == null ? -1 : scanProduction.Outcome.Index)
                        .Stloc(tokenId.GetRef())
                        ;

                    var productionBinding = scanProduction.Joint.The<CilScanProductionDef>();
                    productionBinding.Builder(actionContext);
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
