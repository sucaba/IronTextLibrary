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
using IronText.Build;
using IronText.Logging;
using IronText.Compilation;

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

        class FreezerProcess : IActionCode, ISppfNodeVisitor
        {
            private readonly string       input;
            private Grammar      grammar;
            private IActionCode  code;
            private LanguageData data;
            private SppfNode    root;
            private EmitSyntax  emit;
            private Ref<Args>[] args;
            private ISemanticLoader contextCode;
            private string currentTerminalText;
            private readonly VarsStack localsStack;
            private int currentProdSize;

            public FreezerProcess(string input)
            {
                this.input = input;
                this.code = this;
                this.localsStack = new VarsStack(this.EmitCode);
            }

            public Pipe<TContext> Outcome { get; set; }

            public void Run()
            {
                GetLanguageData();

                this.grammar = data.Grammar;

                BuildTree();

                CompileDelegate();
            }

            private void GetLanguageData()
            {
                Type definitionType = typeof(TContext);
                var source = new CilGrammarSource(definitionType);

                var dataProvider = new LanguageDataProvider(source, false);
                ResourceContext.Instance.LoadOrBuild(dataProvider, out this.data);
                this.data = dataProvider.Resource;
            }

            private void CompileDelegate()
            {
                var cachedMethod = new CachedMethod<Pipe<TContext>>("temp", Compile);
                this.Outcome = cachedMethod.Delegate;
            }

            private void BuildTree()
            {
                using (var interp = new Interpreter<TContext>())
                {
                    this.root = interp.BuildTree(input);
                }
            }

            private EmitSyntax Compile(EmitSyntax emit0, Ref<Args>[] args0)
            {
                this.emit = emit0;
                this.args = args0;

                var globals = new GlobalSemanticLoader(emit, il => il.Ldarg(args[0]), data.Grammar.Globals);
                this.contextCode = new SemanticLoader(globals, emit, null);

                CompileNode(root);
                return emit.Ldarg(0).Ret();
            }

            private void CompileNode(SppfNode node)
            {
                node.Accept(this, false);
            }

            void ISppfNodeVisitor.VisitLeaf(int matcherIndex, string text, Loc location)
            {
                this.currentTerminalText = text;
                var matcher = grammar.Matchers[matcherIndex];

                try
                {
                    TermFactoryGenerator.CompileTermFactory(code, matcher);
                }
                finally
                {
                    this.currentTerminalText = null;
                }

                localsStack.Push();
            }

            void ISppfNodeVisitor.VisitBranch(int productionIndex, SppfNode[] children, Loc location)
            {
                var size = children.Length;
                for (int i = 0; i != size; ++i)
                {
                    CompileNode(children[i]);
                }

                this.currentProdSize = children.Length;
                var production = grammar.Productions[productionIndex];
                ProductionActionGenerator.CompileProduction(
                    Fluent.Create(code),
                    localsStack,
                    production);

                localsStack.Pop(currentProdSize);
                localsStack.Push();
                this.currentProdSize = -1;
            }

            void ISppfNodeVisitor.VisitAlternatives(SppfNode alternatives)
            {
                throw new NotImplementedException();
            }

            IActionCode IActionCode.Emit(Pipe<EmitSyntax> pipe)
            {
                emit = pipe(emit);
                return code;
            }

            IActionCode IActionCode.LdSemantic(string name)
            {
                contextCode.LdSemantic(SemanticRef.ByName(name));
                return code;
            }

            public IActionCode LdActionArgument(int index)
            {
                localsStack.LdSlot((localsStack.Count - currentProdSize) + index);
                return code;
            }

            IActionCode IActionCode.LdActionArgument(int index, Type argType)
            {
                LdActionArgument(index);
                if (argType.IsValueType)
                {
                    this.emit = emit.Unbox_Any(emit.Types.Import(argType));
                }

                return code;
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
                return code.Emit(il => il.Ldstr(new Lib.Ctem.QStr(currentTerminalText)));
            }

            IActionCode IActionCode.ReturnFromAction()
            {
                return code;
            }

            IActionCode IActionCode.SkipAction()
            {
                throw new NotImplementedException();
            }

            private void EmitCode(Pipe<EmitSyntax> fragment)
            {
                this.code = code.Emit(fragment); 
            }
        }
    }
}
