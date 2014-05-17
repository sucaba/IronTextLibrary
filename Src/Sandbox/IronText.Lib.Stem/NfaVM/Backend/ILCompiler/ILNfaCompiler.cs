using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.IL.Generators;
using IronText.Lib.NfaVM.Runtime;
using IronText.Lib.RegularAst;
using IronText.Lib.Shared;
using IronText.Lib.Stem;

namespace IronText.Lib.NfaVM.ILCompiler
{
    class ILCompilerSettings
    {
        public Pipe<EmitSyntax> LdInput;
        public Def<Labels>   SUCCESS;
        public Def<Labels>   FAILURE;
    }

    class ILNfaCompiler : INfaVM
    {
        private static readonly System.Reflection.FieldInfo LabelIndexField = typeof(Thread).GetField("LabelIndex");

        ILCompilerSettings settings;
        private EmitSyntax emit;
        private Def<Labels>  NEXT_THREAD;
        private Def<Labels>  RESUME_ALL;
        private Def<Labels>  NEXT_INPUT;
        private Def<Labels>  THREAD_SUSPEND;
        private Def<Labels>  THREAD_DISPATCH;
        private List<Def<Labels>> indexToLabel = new List<Def<Labels>>();
        private Def<Locals> slots;
        private int slotCount;
        private Def<Locals> labelIndexToLocation;
        private Def<Locals> threadTmp;
        private Def<Locals> intTmp;
        private ArrayLoopGenerator inputPump;
        private StackGenerator runningStack;
        private StackGenerator suspendedStack;
        private StackGenerator matchedStack;
        private StackGenerator tmpStack;

        public ILNfaCompiler(AstNode node, EmitSyntax emit, ILCompilerSettings settings)
        {
            this.Scanner = new StemScanner();

            this.emit = emit;
            this.settings = settings;

            var labels = emit.Labels;
            NEXT_THREAD     = labels.Generate();
            RESUME_ALL      = labels.Generate();
            NEXT_INPUT      = labels.Generate();
            THREAD_SUSPEND  = labels.Generate();
            THREAD_DISPATCH = labels.Generate();

            var LABEL0      = labels.Generate();
            var LABEL1      = labels.Generate();
            var POSTCOMPILEINIT = labels.Generate();
            var POSTCOMPILEINIT_BACK = labels.Generate();

            var locals = emit.Locals;
            slots        = locals.Generate();
            labelIndexToLocation = locals.Generate();
            intTmp       = locals.Generate();
            threadTmp    = locals.Generate();


            INfaVM code = this;

            this.inputPump = new ArrayLoopGenerator(
                       valueType: emit.Types.Int32,
                       ldarray:   settings.LdInput, 
                       body: 
                           il => il
                               .Do(matchedStack.Clear)
                               .Br(RESUME_ALL.GetRef())
                        );

            this.runningStack   = new StackGenerator(emit, typeof(Thread));
            this.suspendedStack = new StackGenerator(emit, typeof(Thread));
            this.matchedStack   = new StackGenerator(emit, typeof(Thread));
            this.tmpStack       = new StackGenerator(emit, typeof(Thread), nullContainer: true); 

            emit
                .Local(intTmp, emit.Types.Int32)
                .Local(slots, typeof(int[]))
                .Local(labelIndexToLocation, typeof(int[]))
                .Br(POSTCOMPILEINIT.GetRef())
                .Label(POSTCOMPILEINIT_BACK)
                .Local(threadTmp, typeof(Thread))
                .Ldloca(threadTmp.GetRef())
                .Initobj(typeof(Thread))
                .Do(inputPump.EmitInitialization)
                ;

            new RegularNfaVMCompiler().Compile(node, code);

            int LabelCount = Math.Max(1, indexToLabel.Count);

            this.runningStack.SetSize(LabelCount);
            this.suspendedStack.SetSize(LabelCount);
            this.matchedStack.SetSize(LabelCount);
            this.tmpStack.SetSize(LabelCount);

            emit
                .Label(POSTCOMPILEINIT)
                .Do(runningStack.Init)
                .Do(suspendedStack.Init)
                .Do(matchedStack.Init)
                .Do(tmpStack.Init)
                .Ldc_I4(slotCount)
                .Newarr(emit.Types.Int32)
                .Stloc(slots.GetRef())

                .Ldc_I4(LabelCount)
                .Newarr(emit.Types.Int32)
                .Stloc(labelIndexToLocation.GetRef())
                // Fill labelIndexToLocation with -1 :
                .Ldc_I4(LabelCount).Stloc(intTmp.GetRef())

                .Label(LABEL1)
                .Ldloc(intTmp.GetRef())
                .Ldc_I4_0()
                .Beq(LABEL0.GetRef())
                .Ldloc(intTmp.GetRef())
                .Ldc_I4(1)
                .Sub()
                .Stloc(intTmp.GetRef())
                .Ldloc(labelIndexToLocation.GetRef())
                .Ldloc(intTmp.GetRef())
                .Ldc_I4(int.MinValue)
                .Stelem_I4()
                .Br(LABEL1.GetRef())
                .Label(LABEL0)
                .Br(POSTCOMPILEINIT_BACK.GetRef());

            // Save thread as suspended (stack should already contain label index to suspend)
            emit.Label(THREAD_SUSPEND);
            StThreadValueByRuntimeLabelIndex();
            emit
                // Don't add thread if same thread (same label index 
                // with current input location) already exists in a list.
                .Ldloc(labelIndexToLocation.GetRef())
                .Ldloca(threadTmp.GetRef())
                .Ldfld(LabelIndexField)
                .Ldelem_I4()
                .Ldloc(inputPump.Index.GetRef())
                .Beq(NEXT_THREAD.GetRef())

                // Mark label index as visited 
                .Ldloc(labelIndexToLocation.GetRef())
                .Ldloca(threadTmp.GetRef())
                .Ldfld(LabelIndexField)
                .Ldloc(inputPump.Index.GetRef())
                .Stelem_I4()
                ;

            suspendedStack.PushFrom(emit, threadTmp);

            emit
                .Br(NEXT_THREAD.GetRef())

                .Label(RESUME_ALL)
                .Swap(suspendedStack.Stack, runningStack.Stack, tmpStack.Stack)
                .Swap(suspendedStack.Index, runningStack.Index, tmpStack.Index)
                .Label(NEXT_THREAD)
                ;

            runningStack
                .StackLoop(
                    emit,
                    (emit2, thread) =>
                    {
                        emit2
                            .Ldloca(thread.GetRef())
                            .Ldfld(LabelIndexField)
                            .Label(THREAD_DISPATCH)
                            .Switch(indexToLabel.Select(def => def.GetRef()).ToArray())
                            .Br(settings.FAILURE.GetRef());
                    })
                    ;
            emit
                .Br(NEXT_INPUT.GetRef())
                ;

            emit
                .Label(NEXT_INPUT)
                ;

            inputPump.EmitLoopPass(emit, false);

            emit
                // Check if there are matched threads:
                .Do(matchedStack.LdCount)
                .Ldc_I4_0()
                .Beq(settings.FAILURE.GetRef())
                .Br(settings.SUCCESS.GetRef());
        }

        public StemScanner Scanner { get; private set; }

        public OnDemandNs<Labels> Labels { get { return this.emit.Labels; } }

        public void Program(Zom_<INfaVM> entries) { }

        public INfaVM Label(Def<Labels> label)
        {
            emit.Label(label);
            if (!indexToLabel.Contains(label))
            {
                indexToLabel.Add(label);
            }

            return this;
        }

        public INfaVM Fetch()
        {
            var nextInstructionLabel = emit.Labels.Generate();
            int nextInstructionLabelIndex = indexToLabel.Count;
            indexToLabel.Add(nextInstructionLabel);

            emit
                .Ldc_I4(nextInstructionLabelIndex)
                .Br(THREAD_SUSPEND.GetRef())
                .Label(nextInstructionLabel);
            return this;
        }

        public INfaVM IsA(int expected) 
        {
            emit
                .Ldloc(inputPump.Value.GetRef())
                .Ldc_I4(expected)
                .Bne_Un(NEXT_THREAD.GetRef());
            return this;
        }

        public INfaVM Match()
        {
            matchedStack.PushFrom(emit, threadTmp);
            emit.Br(NEXT_THREAD.GetRef());
            return this;
        }

        public INfaVM Jmp(Ref<Labels> label)
        {
            emit
                .Br(label);
            return this;
        }

        public INfaVM Fork(Ref<Labels> label)
        {
            StThreadValue(label);
            runningStack.PushFrom(emit, threadTmp);
            return this;
        }

        public INfaVM Save(int slotIndex)
        {
            slotCount = Math.Max(slotCount, slotIndex + 1);

            // TODO: clone on modification
            emit
                .Ldloc(slots.GetRef())
                .Ldc_I4(slotIndex)
                .Ldloc(inputPump.Index.GetRef())
                .Stelem_I4();
            return this;
        }

        public int IntNum(Num num) { return int.Parse(num.Text); }

        private int IndexOfLabel(Ref<Labels> label) 
        { 
            int result = indexToLabel.IndexOf(label.Def);
            if (result < 0)
            {
                throw new InvalidOperationException("Internal Error: Label was not registered.");
            }

            return result;
        }

        private void StThreadValue(Ref<Labels> label)
        {
            if (!indexToLabel.Contains(label.Def))
            {
                indexToLabel.Add(label.Def);
            }

            emit
                .Ldloca(threadTmp.GetRef())
                .Ldc_I4(IndexOfLabel(label))
                .Stfld(LabelIndexField)
                ;
        }

        private void StThreadValueByRuntimeLabelIndex()
        {
            emit
                .Stloc(intTmp.GetRef())
                .Ldloca(threadTmp.GetRef())
                .Ldloc(intTmp.GetRef())
                .Stfld(LabelIndexField)
                ;
        }
    }
}
