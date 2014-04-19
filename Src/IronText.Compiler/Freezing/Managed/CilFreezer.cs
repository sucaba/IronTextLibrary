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
                                    data.Grammar.GlobalContextProvider);

                CompileNode(root);
                return emit.Ldarg(0).Ret();
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
                code = ProductionActionGenerator.CompileProduction(code, production);

                PopSlots(count);
                PushSlot();
            }

            private void PopSlots(int count)
            {
                for (int i = 0; i != count; ++i)
                {
                    freeSlotLocals.Push(slotLocals[i]);
                }

                slotLocals.RemoveRange(0, count);
            }

            private void CompileTerminalNode(SppfNode node)
            {
                var matcher = grammar.Matchers[node.MatcherIndex];
                TermFactoryGenerator.CompileTermFactory(code, matcher);

                PushSlot();
            }

            private void PushSlot()
            {
                if (freeSlotLocals.Count == 0)
                {
                    // Add one more local variable for storing argument.
                    code = code.Emit(
                        il =>
                        {
                            var l = il.Locals.Generate("slot" + currentSlotCount);
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
                throw new NotImplementedException();
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
