using System;

namespace IronText.Runtime.Semantics
{
    [Serializable]
    public struct RuntimeReference
    {
        public static RuntimeReference Inh(int offset, int index)
        {
            return new RuntimeReference(RuntimePropertyKind.Inherited, offset, index);
        }

        public static RuntimeReference Synth(int offset, int index)
        {
            return new RuntimeReference(RuntimePropertyKind.Synthesized, offset, index);
        }

        public RuntimeReference(RuntimePropertyKind kind, int offset, int index)
        {
            this.Kind   = kind;
            this.Offset = offset;
            this.Index  = index;
        }

        public RuntimePropertyKind  Kind    { get; private set; }

        public int                  Offset  { get; private set; }

        public int                  Index   { get; private set; }
    }
}
