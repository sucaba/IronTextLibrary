using System;
using System.Runtime.InteropServices;

namespace IronText.Runtime
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct ParserAction : IEquatable<ParserAction>
    {
        public static readonly ParserAction FailAction = new ParserAction();
        public static readonly ParserAction ExitAction = new ParserAction(ParserActionKind.Exit);
        public static readonly ParserAction ContinueAction = new ParserAction(ParserActionKind.Restart);
        public static readonly ParserAction InternalErrorAction = new ParserAction(ParserActionKind.InternalError);

        private const int KindStartBit      = 0;
        private const int KindBits          = 4;

        private const int Value1StartBit    = KindStartBit + KindBits;
        private const int Value1Bits        = 18;

        private const int Value2StartBit    = Value1StartBit + Value1Bits;
        private const int Value2Bits        = 9;
        public  const int Value2Max         = ((1 << Value2Bits) - 1);

        private const int KindMask          = ((1 << KindBits) - 1)   << KindStartBit; 
        private const int Value1Mask        = ((1 << Value1Bits) - 1) << Value1StartBit; 

        public const int Value1Max          = ((1 << Value1Bits) - 1);
        private const int Value2Mask        = ((1 << Value2Bits) - 1) << Value2StartBit; 

        public const int FailActionCell     = 0;

        [FieldOffset(0)]
        public ParserActionKind Kind;

        [FieldOffset(sizeof(ParserActionKind))]
        public int              Value1;

        [FieldOffset(sizeof(ParserActionKind))]
        public int              ResolvedToken;

        [FieldOffset(sizeof(ParserActionKind))]
        public int              State;

        [FieldOffset(sizeof(ParserActionKind))]
        public int              ProductionId;

        [FieldOffset(sizeof(ParserActionKind) + sizeof(int))]
        public short Value2;

        /// <summary>
        /// Count of conflict in transition
        /// </summary>
        [FieldOffset(sizeof(ParserActionKind) + sizeof(int))]
        public short ConflictCount;

        public ParserAction(ParserActionKind kind, int value1 = 0, short value2 = 0)
        {
            this.ResolvedToken = 0;
            this.State = 0;
            this.ProductionId = 0;
            this.ConflictCount = 0;

            this.Kind   = kind;
            this.Value1 = value1;
            this.Value2 = value2;
        }


        public static bool operator==(ParserAction x, ParserAction y)
        {
            return x.Value1 == y.Value1 
                && x.Kind == y.Kind 
                && x.Value2 == y.Value2;
        }

        public static bool operator!=(ParserAction x, ParserAction y)
        {
            return !(x == y);
        }

        public bool Equals(ParserAction other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == (ParserAction)obj;
        }

        public override int GetHashCode()
        {
            unchecked { return (int)Kind + (int)Value1; }
        }

        public override string ToString()
        {
            return new[] { "F", "S", "R", "E", "C", "A", "SR" }[(int)Kind] + Value1;
        }

        public static int Encode(ParserAction action)
        {
            int result = ((byte)action.Kind) 
                | (action.Value1 << Value1StartBit)
                | (action.Value2 << Value2StartBit)
                ;
            return result;
        }

        public static int Encode(ParserActionKind kind, int value1)
        {
            if (value1 > Value1Max)
            {
                throw new ArgumentOutOfRangeException("value1");
            }

            int result = ((byte)kind) | (value1 << Value1StartBit);
            return result;
        }

        public static int EncodeModifedReduce(int ruleIndex, int size)
        {
            if (ruleIndex > Value1Max)
            {
                throw new ArgumentOutOfRangeException("ruleIndex");
            }

            if (size > Value2Max)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            int result = ((byte)ParserActionKind.Reduce) 
                        | (ruleIndex << Value1StartBit) 
                        | (size << Value2StartBit);
            return result;
        }

        public static ParserAction Decode(int cell)
        {
            ParserAction result = new ParserAction();
            Decode(cell, ref result);
            return result;
        }

        public static ParserActionKind Decode(int cell, ref ParserAction result)
        {
            ParserActionKind kind = (ParserActionKind)(cell & KindMask);
            result.Kind = kind;

            if (kind != ParserActionKind.Fail)
            {
                result.Value1 = (cell & Value1Mask) >> Value1StartBit;
                result.Value2 = (short)((cell & Value2Mask) >> Value2StartBit);
            }

            return kind;
        }

        public static ParserActionKind GetKind(int cell)
        {
            return (ParserActionKind)(cell & KindMask);
        }

        public static bool IsShift(int cell)
        {
            return IsShift((ParserActionKind)(cell & KindMask));
        }

        public bool IsShiftAction => IsShift(Kind);

        public static bool IsShift(ParserActionKind kind)
        {
            return kind == ParserActionKind.Shift;
        }

        public static int GetId(int cell)
        {
            return (cell & Value1Mask) >> Value1StartBit;
        }
    }
}
