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
            private const string SlotLocalPrefix = "slot";

            private readonly string       input;
            private Grammar      grammar;
            private IActionCode  code;
            private LanguageData data;
            private SppfNode    root;
            private EmitSyntax  emit;
            private Ref<Args>[] args;
            private IContextCode contextCode;
            private readonly List<Ref<Locals>> slotLocals = new List<Ref<Locals>>();
            private readonly Stack<Ref<Locals>> freeSlotLocals = new Stack<Ref<Locals>>();
            private int currentSlotCount = 0;
            private string currentTerminalText;

            public FreezerProcess(string input)
            {
                this.input = input;
                this.code = this; 
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

                this.contextCode = new ContextCode(
                                    emit,
                                    il => il.Ldarg(args[0]),
                                    null,
                                    data,
                                    data.Grammar.Globals);

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

                PushSlot();
            }

            void ISppfNodeVisitor.VisitBranch(int productionIndex, SppfNode[] children, Loc location)
            {
                int count = children.Length;
                for (int i = 0; i != count; ++i)
                {
                    CompileNode(children[i]);
                }

                var production = grammar.Productions[productionIndex];
                code = ProductionActionGenerator.CompileProduction(code, production);

                PopSlots(count);
                PushSlot();
            }

            void ISppfNodeVisitor.VisitAlternatives(SppfNode alternatives)
            {
                throw new NotImplementedException();
            }

            private void PopSlots(int count)
            {
                for (int i = 0; i != count; ++i)
                {
                    freeSlotLocals.Push(slotLocals[i]);
                }

                slotLocals.RemoveRange(0, count);
            }

            private void PushSlot()
            {
                if (freeSlotLocals.Count == 0)
                {
                    // Add one more local variable for storing argument.
                    code = code.Emit(
                        il =>
                        {
                            var l = il.Locals.Generate(SlotLocalPrefix + currentSlotCount);
                            freeSlotLocals.Push(l.GetRef());
                            return il.Local(l, il.Types.Object);
                        });

                    ++currentSlotCount;
                }

                var slot = freeSlotLocals.Pop();
                code = code.Emit(il => il.Stloc(slot));
                slotLocals.Add(slot);
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
                return code.Emit(il => il.Ldloc(slotLocals[index]));
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
        }
    }
}
