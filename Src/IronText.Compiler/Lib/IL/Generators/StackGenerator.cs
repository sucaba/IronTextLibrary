using System;
using IronText.Lib.Shared;

namespace IronText.Lib.IL.Generators
{
    public class StackGenerator
    {
        private Def<Locals> stack;
        private Def<Locals> index;
        private readonly Ref<Types> itemType;
        private readonly Ref<Types> stackType;
        private int stackSize;
        private readonly bool nullContainer;

        public StackGenerator(EmitSyntax emit, Type itemType, bool nullContainer = false)
        {
            this.itemType = emit.Types.Import(itemType);
            this.stackType = emit.Types.Import(itemType.MakeArrayType());
            this.nullContainer = nullContainer;

            this.stack = emit.Locals.Generate();
            this.index = emit.Locals.Generate();
            emit
                .Local(stack, stackType)
                .Local(index, emit.Types.Int32)
                ;
        }

        public void SetSize(int size)
        {
            this.stackSize = size;
        }
        
        public Def<Locals> Stack { get { return stack; } }
        public Def<Locals> Index { get { return index; } }
        public Ref<Types> StackType { get { return stackType; } }
        public Ref<Types> ItemType { get { return itemType; } }

        public EmitSyntax Init(EmitSyntax emit)
        {
            emit = emit
                .Ldc_I4_0()
                .Stloc(index.GetRef())
                ;

            if (!nullContainer)
            {
                emit = emit
                    .Ldc_I4(this.stackSize)
                    .Newarr(itemType)
                    .Stloc(stack.GetRef())
                    ;
            }

            return emit;
        }

        public void PushFrom(EmitSyntax emit, Def<Locals> local)
        {
            emit
                .Ldloc(stack.GetRef())
                .Ldloc(index.GetRef())
                .Ldloc(local.GetRef())
                .Stelem(itemType)
                .Ldloc(index.GetRef())
                .Ldc_I4_1()
                .Add()
                .Stloc(index.GetRef())
                ;
        }

        /// <summary>
        /// Pops item and puts on the CLR stack
        /// </summary>
        /// <param name="emit"></param>
        public EmitSyntax Pop(EmitSyntax emit)
        {
            return emit
                .Ldloc(index.GetRef())
                .Ldc_I4_1()
                .Sub()
                .Stloc(index.GetRef())
                .Ldloc(stack.GetRef())
                .Ldloc(index.GetRef())
                .Ldelem(itemType)
                ;
        }

        public EmitSyntax StackLoop(EmitSyntax emit, Action<EmitSyntax, Def<Locals>> emitLoopBody)
        {
            var labels = emit.Labels;
            var START = labels.Generate();
            var END = labels.Generate();

            var item = emit.Locals.Generate();

            emit
                .Local(item, itemType)
                .Label(START)
                .Ldloc(index.GetRef())
                .Ldc_I4(0)
                .Beq(END.GetRef())
                .Do(Pop)
                .Stloc(item.GetRef())
                ;

            emitLoopBody(emit, item);

            emit
                .Br(START.GetRef())
                .Label(END)
                .Nop();

            return emit;
        }

        public void LdInstance(EmitSyntax emit)
        {
            emit
                .Ldloc(stack.GetRef())
                ;
        }

        public EmitSyntax Clear(EmitSyntax emit)
        {
            return emit
                .Ldc_I4_0()
                .Stloc(index.GetRef())
                ;
        }

        public EmitSyntax LdCount(EmitSyntax emit)
        {
            return emit.Ldloc(index.GetRef());
        }
    }
}
