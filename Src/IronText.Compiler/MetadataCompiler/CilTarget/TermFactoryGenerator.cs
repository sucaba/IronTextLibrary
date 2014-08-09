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

            var RETURN  = labels.Generate();
            var DEFAULT = labels.Generate();


            var actionLabels = data.Grammar.Matchers.CreateCompatibleArray(DEFAULT.GetRef());
            int first = data.Grammar.Matchers.StartIndex;
            int last  = data.Grammar.Matchers.LastIndex;

            for (int i = first; i != last; ++i)
            {
                actionLabels[i] = labels.Generate().GetRef();
            }

            emit
                .Do(ldAction)
                .Switch(actionLabels)
                ;

            var globals = new GlobalSemanticLoader(emit, il => il.Do(ldRootContext), data.Grammar.Globals);
            ISemanticLoader localSemanticCode = new StackSemanticLoader(globals, emit, null);
            IActionCode code = new MatcherCode(emit, localSemanticCode, ldText, RETURN.GetRef());

            foreach (var matcher in data.Grammar.Matchers)
            {
                emit.Label(actionLabels[matcher.Index].Def);

                code = CompileTermFactory(code, matcher);
            }

            // Load null value for incorrectly implemented actions
            emit
                .Label(DEFAULT)
                .Ldnull();

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
