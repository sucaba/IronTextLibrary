using System.Diagnostics;
using System.Linq;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;
using IronText.MetadataCompiler.CilTarget;
using IronText.Framework;
using IronText.Compilation;
using System;

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

            Def<Args> pargs         = args.Args.Generate("pargs"); 
            Def<Args> ruleId        = args.Args.Generate("ruleId");
            Def<Args> ruleArgs      = args.Args.Generate("ruleArgs");
            Def<Args> argsStart     = args.Args.Generate("argsStart");
            Def<Args> ctx           = args.Args.Generate("rootContext");
            Def<Args> stackLookback = args.Args.Generate("stackLookback");

            var emit = args
                    .Argument(context.Types.Import(typeof(ProductionActionArgs)), pargs)
                    .EndArgs()

                .BeginBody();

            BuildBody(
                emit, 
                data, 
                pargs.GetRef());

            return emit.EndBody();
        }

        public void BuildBody(EmitSyntax emit, LanguageData data, Ref<Args>[] args)
        {
            BuildBody(emit, data, pargs: args[0]);
        }

        private void BuildBody(EmitSyntax emit, LanguageData data, Ref<Args> pargs)
        {
            var varStack = new VarsStack(Fluent.Create(emit));

            Def<Labels> returnLabel = emit.Labels.Generate();

            Pipe<EmitSyntax> ldCtx = il => il
                                        .Ldarg(pargs)
                                        .Ldprop((ProductionActionArgs a) => a.Context);
            Pipe<EmitSyntax> ldRuleId = il => il
                                        .Ldarg(pargs)
                                        .Ldprop((ProductionActionArgs a) => a.ProductionIndex);
            var globalSemanticCode = new GlobalSemanticLoader(emit, ldCtx, data.Grammar.Globals);

            var defaultLabel = emit.Labels.Generate();
            var endWithSingleResultLabel = emit.Labels.Generate();

            var jumpTable = data.Grammar.Productions.CreateCompatibleArray(defaultLabel.GetRef());
            int first = data.Grammar.Productions.StartIndex;
            int last  = data.Grammar.Productions.Count;
            for (int i = first; i != last; ++i)
            {
                jumpTable[i] = emit.Labels.Generate().GetRef();
            }

            emit
                .Do(ldRuleId)
                .Switch(jumpTable)
                .Br(defaultLabel.GetRef());

            foreach (var prod in data.Grammar.Productions)
            {
                emit.Label(jumpTable[prod.Index].Def);

                CompileProduction(emit, data, pargs, returnLabel, globalSemanticCode, prod, varStack);
                varStack.LdLastSlot();
                varStack.Pop(1);

                emit.Br(endWithSingleResultLabel.GetRef());
            }

            emit
                .Label(defaultLabel)
                .Ldnull()
                .Label(endWithSingleResultLabel)
                .Label(returnLabel)
                .Ret();
        }

        private static void CompileProduction(
            EmitSyntax      emit,
            LanguageData    data,
            Ref<Args>       pargs,
            Def<Labels>     returnLabel,
            ISemanticLoader globals,
            Production      prod,
            VarsStack       varsStack)
        {
            Pipe<EmitSyntax> ldLookback = il => il
                                        .Ldarg(pargs)
                                        .Ldprop((ProductionActionArgs a) => a.Lookback);
            var locals = new StackSemanticLoader(
                globals,
                emit,
                ldLookback,
                data.SemanticBindings,
                stackShift: prod.InputLength);

            Func<int,Pipe<EmitSyntax>> ldSyntaxArg = bo => il => il
                                        .Ldarg(pargs)
                                        .Ldc_I4(bo)
                                        .Call((ProductionActionArgs _0, int _1) 
                                        => _0.GetSyntaxArgByBackOffset(_1));

            int varsStackStart = varsStack.Count;
            int backOffset = prod.InputLength;
            foreach (var arg in prod.Input)
            {
                emit = emit
                    .Do(ldSyntaxArg(backOffset))
                    .Ldfld((ActionNode msg) => msg.Value)
                    ;

                varsStack.Push();

                --backOffset;
            }

            var emitCoder = Fluent.Create(emit);

            // Build inlined productions 
            var compiler = new ProductionCompiler(emitCoder, varsStack, globals);
            compiler.Execute(prod);


            var coder = Fluent.Create<IActionCode>(new ProductionCode(
                    emitCoder,
                    locals,
                    varsStack,
                    varsStackStart));

            CompileProduction(coder, varsStack, varsStackStart, prod);
        }

        public static void CompileProduction(
            Fluent<IActionCode> coder,
            VarsStack           varStack,
            int                 varsStackStart, 
            Production          prod)
        {
            var bindings = prod.Joint.All<CilProduction>();
            if (!bindings.Any())
            {
                if (prod.IsAugmented)
                {
                    coder.Do(c => c.Emit(il => il.Ldnull()));
                }
                else if (prod.HasIdentityAction)
                {
                    coder.Do(c => c.LdActionArgument(0));
                }
                else
                {
                    var msg = string.Format("Production '{0}' has no associated managed action.", prod);
                    throw new InvalidOperationException(msg);
                }
            }
            else
            {
                bool first = true;
                foreach (var binding in bindings)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        // Result of this rule supersedes result of the prvious one
                        coder.Do(c => c
                            .Emit(il => il.Pop()));
                    }

                    coder.Do(c => c
                        .LdSemantic(binding.Context.UniqueName)
                        .Do(binding.ActionBuilder))
                        ;
                }
            }

            varStack.RemoveRange(varsStackStart, prod.ChildComponents.Length);
            varStack.InsertAt(varsStackStart);
        }
    }
}
