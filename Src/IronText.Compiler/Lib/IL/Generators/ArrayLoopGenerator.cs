using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL.Generators
{
    public class ArrayLoopGenerator : ILoopGenerator
    {
        private readonly Ref<Types> valueType;
        private readonly Pipe<EmitSyntax> body;
        private readonly Pipe<EmitSyntax> ldarray;
        private Def<Locals> count;
        private Def<Labels> START;
        private Def<Labels> END;

        public ArrayLoopGenerator(Ref<Types> valueType, Pipe<EmitSyntax> ldarray, Pipe<EmitSyntax> body)
        {
            this.valueType = valueType;
            this.body = body;
            this.ldarray = ldarray;
        }

        public Def<Locals> Index { get; set; }
        public Def<Locals> Value { get; set; }

        public EmitSyntax EmitInitialization(EmitSyntax emit)
        {
            var types = emit.Types;

            var labels = emit.Labels;
            END = labels.Generate();
            START = labels.Generate();

            var locals = emit.Locals;
            count = locals.Generate();
            if (Index == null)
            {
                Index = locals.Generate();
                emit.Local(Index, types.Int32);
            }

            if (Value == null)
            {
                Value = locals.Generate();
                emit
                    .Local(Value, valueType)
                    .Ldc_I4(int.MinValue)
                    .Stloc(Value.GetRef());
            }

            emit
                // int count;
                .Local(count, types.Int32);

            // count = array.Length
            ldarray(emit);
            return emit
                .Ldlen()
                .Stloc(count.GetRef())

                // Index = 0
                .Ldc_I4(-1)
                .Stloc(Index.GetRef())
                // Value = -1
                .Ldc_I4(-1)
                .Stloc(Value.GetRef());
                ;
        }

        public EmitSyntax EmitLoopPass(EmitSyntax emit, bool loop)
        {
            emit.Label(START);

            FetchNext(emit, END);
            body(emit);

            if (loop)
            {
                emit.Br(START.GetRef());
            }

            return emit
                .Label(END)
                .Nop();
        }

        private void FetchNext(EmitSyntax emit, Def<Labels> END_OF_INPUT)
        {
            emit
                .Ldloc(Index.GetRef())
                .Ldc_I4_1()
                .Add()
                .Stloc(Index.GetRef())
                // ++Index
                // if (Index == count) goto END
                .Ldloc(Index.GetRef())
                .Ldloc(count.GetRef())
                .Beq(END_OF_INPUT.GetRef())
                .Do(ldarray)
                .Ldloc(Index.GetRef())
                .Ldelem(valueType)
                .Stloc(Value.GetRef())
                ;
        }

        public void LdValue(EmitSyntax emit)
        {
            emit.Ldloc(Value.GetRef());
        }
    }
}
