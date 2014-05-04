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
        private readonly LanguageData data;

        public TermFactoryGenerator(LanguageData data)
        {
            this.data = data;
        }

        public EmitSyntax Build(EmitSyntax emit, Ref<Args>[] args)
        {
            return Build(
                emit, 
                il => il.Ldarg(args[0]),
                il => il.Ldarg(args[1]),
                il => il.Ldarg(args[2]));
        }

        public EmitSyntax Build(
            EmitSyntax       emit,
            Pipe<EmitSyntax> ldRootContext,
            Pipe<EmitSyntax> ldAction,
            Pipe<EmitSyntax> ldText)
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

            var globals = new GlobalSemanticCode(emit, il => il.Do(ldRootContext), data.Grammar.Globals);
            ISemanticCode localSemanticCode  = new SemanticCode(globals, emit, null, data);
            IActionCode code = new MatcherCode(emit, localSemanticCode, ldText, RETURN.GetRef());

            foreach (var matcher in data.Grammar.Matchers)
            {
                emit.Label(actionLabels[matcher.Index].Def);

                code = CompileTermFactory(code, matcher);
            }

            // Load null value for incorrectly implemented actions
            emit.Ldnull();

            return emit
                .Label(RETURN)
                .Ret()
                ;
        }

        public static IActionCode CompileTermFactory(IActionCode code, Matcher matcher)
        {
            var binding = matcher.Joint.The<CilMatcher>();
            code = code
                .LdSemantic(binding.Context.UniqueName)
                .Do(binding.ActionBuilder);
            return code;
        }
    }
}
