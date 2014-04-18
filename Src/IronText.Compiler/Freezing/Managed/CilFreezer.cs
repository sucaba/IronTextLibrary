using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Runtime;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.MetadataCompiler;

namespace IronText.Freezing.Managed
{
    public class CilFreezer<TContext> : IDisposable
        where TContext : class
    {
        public Pipe<TContext> Compile(string input)
        {
            var process = new FreezerProcess(input);
            process.Run();
            return process.Outcome;
        }

        ~CilFreezer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
        }

        class FreezerProcess : IActionCode
        {
            private readonly string       input;
            private readonly Grammar      grammar;
            private readonly IActionCode  code;
            private readonly LanguageData data;
            private SppfNode    root;
            private EmitSyntax  emit;
            private Ref<Args>[] args;
            private IContextCode contextCode;

            public FreezerProcess(/*LanguageData data,*/ string input)
            {
                this.input = input;
                this.code = this; 
//                this.data = data;
//                this.grammar = data.Grammar;
            }

            public Pipe<TContext> Outcome { get; set; }

            public void Run()
            {
                this.root    = BuildTree(input);

                CompileDelegate();
            }

            private void CompileDelegate()
            {
                var cachedMethod = new CachedMethod<Pipe<TContext>>("temp", (emit, args) => emit.Ldnull().Ret());
                this.Outcome = cachedMethod.Delegate;
            }

            private SppfNode BuildTree(string input)
            {
                using (var interp = new Interpreter<TContext>())
                {
                    return interp.BuildTree(input);
                }
            }

            private void Compile(EmitSyntax emit0, Ref<Args>[] args0, SppfNode root)
            {
                this.emit = emit0;
                this.args = args0;

                this.contextCode = new ContextCode(
                                    emit,
                                    il => il.Ldarg(args[0]),
                                    null,
                                    data,
                                    data.Grammar.GlobalContextProvider);

                CompileNode(root);
                emit = emit.Ret();
            }

            private void CompileNode(SppfNode node)
            {
                if (node.IsTerminal)
                {
                    CompileTerminalNode(node);
                }
                else
                {
                    CompileProductionNode(node);
                }
            }

            private void CompileProductionNode(SppfNode node)
            {
                int count = node.Children.Length;
                for (int i = 0; i != count; ++i)
                {
                    CompileNode(node.Children[i]);
                }

                var production = grammar.Productions[node.ProductionIndex];
            }

            private void CompileTerminalNode(SppfNode node)
            {
                var matcher = grammar.Matchers[node.MatcherIndex];
                var binding = matcher.Joint.The<CilMatcher>();

                 code
                    .Do(binding.Context.Load)
                    .Do(binding.ActionBuilder);
            }

            IActionCode IActionCode.Emit(Pipe<EmitSyntax> pipe)
            {
                emit = pipe(emit);
                return code;
            }

            IActionCode IActionCode.LdContext(string contextName)
            {
                contextCode.LdContext(contextName);
                return code;
            }

            IActionCode IActionCode.LdActionArgument(int index, Type argType)
            {
                throw new NotImplementedException();
            }

            IActionCode IActionCode.LdMergerOldValue()
            {
                throw new NotImplementedException();
            }

            IActionCode IActionCode.LdMergerNewValue()
            {
                throw new NotImplementedException();
            }

            IActionCode IActionCode.LdMatcherTokenString()
            {
                throw new NotImplementedException();
            }

            IActionCode IActionCode.ReturnFromAction()
            {
                throw new NotImplementedException();
            }

            IActionCode IActionCode.SkipAction()
            {
                throw new NotImplementedException();
            }
        }
    }
}
