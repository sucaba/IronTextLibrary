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
        public static readonly ParserInstruction Pop                 = new ParserInstruction(ParserOperation.Pop);

        public static ParserInstruction Shift(int state) =>
            new ParserInstruction(ParserOperation.Shift, state);

        public static ParserInstruction Reduce(int production) =>
            new ParserInstruction(ParserOperation.Reduce, production);

        public static ParserInstruction ReduceGoto(int production, int nextState) =>
            new ParserInstruction(ParserOperation.ReduceGoto, production, nextState);
        
        public static ParserInstruction PushGoto(int pushState, int nextState) =>
            new ParserInstruction(ParserOperation.PushGoto, nextState, pushState);

        public static ParserInstruction Return(int producedToken) =>
            new ParserInstruction(ParserOperation.Pop, producedToken);

        public static ParserInstruction Resolve(int token) =>
            new ParserInstruction(ParserOperation.Resolve, token);

        public static ParserInstruction Fork(int instructionPosition) =>
            new ParserInstruction(ParserOperation.Fork, instructionPosition);

        public static ParserInstruction Dispatch(int state) =>
            new ParserInstruction(ParserOperation.Dispatch, state);

        [FieldOffset(0)]
        public ParserOperation  Operation;

        [FieldOffset(sizeof(ParserOperation))]
        public int              Argument;

        public int              ForkPosition => Argument;

        [FieldOffset(sizeof(ParserOperation))]
        public int              ResolvedToken;

        [FieldOffset(sizeof(ParserOperation))]
        public int              State;

        [FieldOffset(sizeof(ParserOperation))]
        public int              Production;

        [FieldOffset(sizeof(ParserOperation) + sizeof(int))]
        public int              Argument2;

        [FieldOffset(sizeof(ParserOperation) + sizeof(int))]
        public int              PushState;

        public ParserInstruction(ParserOperation op, int argument = 0, int argument2 = 0)
        {
            this.ResolvedToken = 0;
            this.State         = 0;
            this.Production    = 0;

            this.Operation     = op;
            this.Argument      = argument;
            this.Argument2     = argument2;
            this.PushState     = argument2;
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

        public override string ToString()
        {
            switch (Operation)
            {
                case ParserOperation.Dispatch:
                    return $"dispatch-{State}";
                case ParserOperation.Accept:
                    return "accept";
                case ParserOperation.Exit:
                    return "exit";
                case ParserOperation.Fail:
                    return "fail";
                case ParserOperation.Fork:
                    return $"fork ${ForkPosition}";
                case ParserOperation.InternalError:
                    return "inernal-error";
                case ParserOperation.Pop:
                    return "pop";
                case ParserOperation.PushGoto:
                    return $"push S{PushState} goto S{State}";
                case ParserOperation.Reduce:
                    return $"reduce P{Production}";
                case ParserOperation.ReduceGoto:
                    return $"reduce P{Production}, goto S{Argument2}";
                case ParserOperation.Resolve:
                    return $"resolve T{ResolvedToken}";
                case ParserOperation.Shift:
                    return $"shift S{State}";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
