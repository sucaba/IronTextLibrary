using System.Runtime.InteropServices;

namespace IronText.Runtime.Inlining
{
    [StructLayout(LayoutKind.Explicit)]
    public struct InlinedAction
    {
        [FieldOffset(0)]
        public InlinedActionOp Op;

        [FieldOffset(sizeof(InlinedActionOp))]
        public int StackSize;

        [FieldOffset(sizeof(InlinedActionOp))]
        public int BackOffset;

        [FieldOffset(sizeof(InlinedActionOp))]
        public int ProductionIndex;

        [FieldOffset(sizeof(InlinedActionOp))]
        public int ShiftedState;
    }
}
