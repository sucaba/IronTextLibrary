using System;
using System.Runtime.InteropServices;

namespace IronText.Runtime
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct ParserInstruction : IEquatable<ParserInstruction>
    {
        public static readonly ParserInstruction AcceptAction        = new ParserInstruction(ParserOperation.Accept);
        public static readonly ParserInstruction FailAction          = new ParserInstruction();
        public static readonly ParserInstruction ExitAction          = new ParserInstruction(ParserOperation.Exit);
        public static readonly ParserInstruction InternalErrorAction = new ParserInstruction(ParserOperation.InternalError);

        public static ParserInstruction Shift(int state) =>
            new ParserInstruction(ParserOperation.Shift, state);

        public static ParserInstruction Reduce(int production) =>
            new ParserInstruction(ParserOperation.Reduce, production);
        
        public static ParserInstruction Return(int producedToken) =>
            new ParserInstruction(ParserOperation.Return, producedToken);

        public static ParserInstruction ForceState(int state) =>
            new ParserInstruction(ParserOperation.ForceState, state);

        public static ParserInstruction Resolve(int token) =>
            new ParserInstruction(ParserOperation.Resolve, token);

        public static ParserInstruction Fork(int instructionPosition) =>
            new ParserInstruction(ParserOperation.Fork, instructionPosition);

        [FieldOffset(0)]
        public ParserOperation  Operation;

        [FieldOffset(sizeof(ParserOperation))]
        public int              Argument;

        [FieldOffset(sizeof(ParserOperation))]
        public int              ResolvedToken;

        [FieldOffset(sizeof(ParserOperation))]
        public int              State;

        [FieldOffset(sizeof(ParserOperation))]
        public int              Production;

        public ParserInstruction(ParserOperation op, int argument = 0)
        {
            this.ResolvedToken = 0;
            this.State         = 0;
            this.Production    = 0;

            this.Operation     = op;
            this.Argument      = argument;
        }

        public static bool operator ==(ParserInstruction x, ParserInstruction y) =>
            x.Argument == y.Argument
            && x.Operation == y.Operation;

        public static bool operator !=(ParserInstruction x, ParserInstruction y) =>
            !(x == y);

        public bool Equals(ParserInstruction other) =>
            this == other;

        public override bool Equals(object obj) =>
            this == (ParserInstruction)obj;

        public override int GetHashCode() =>
            unchecked(((int)Operation + Argument));

        public override string ToString() => $"{Operation}-{Argument}";
    }
}
