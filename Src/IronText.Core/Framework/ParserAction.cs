using System;
using System.Runtime.InteropServices;

namespace IronText.Framework
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct ParserAction
    {
        public static readonly ParserAction FailAction = new ParserAction();

        private const int KindStartBit      = 0;
        private const int KindBits          = 3;

        private const int Value1StartBit    = KindStartBit + KindBits;
        private const int Value1Bits        = 19;

        private const int Value2StartBit    = Value1StartBit + Value1Bits;
        private const int Value2Bits        = 9;
        public  const int Value2Max         = ((1 << Value2Bits) - 1);

        private const int KindMask          = ((1 << KindBits) - 1)   << KindStartBit; 
        private const int Value1Mask        = ((1 << Value1Bits) - 1) << Value1StartBit; 

        public const int Value1Max          = ((1 << Value1Bits) - 1);
        private const int Value2Mask        = ((1 << Value2Bits) - 1) << Value2StartBit; 
        public const int FailActionCell = 0;

        [FieldOffset(0)]
        public ParserActionKind Kind;

        [FieldOffset(sizeof(ParserActionKind))]
        public int              Value1;

        [FieldOffset(sizeof(ParserActionKind))]
        public int              RolvedToken;

        [FieldOffset(sizeof(ParserActionKind))]
        public int              State;

        [FieldOffset(sizeof(ParserActionKind))]
        public int              Rule;

        [FieldOffset(sizeof(ParserActionKind))]
        public int              ExternalToken;

        [FieldOffset(sizeof(ParserActionKind) + sizeof(int))]
        public short Value2;

        /// <summary>
        /// Size of the reduction for reduction modified DFA
        /// </summary>
        [FieldOffset(sizeof(ParserActionKind) + sizeof(int))]
        public short Size;

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
                | (action.Size << Value2StartBit)
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
                result.Size = (short)((cell & Value2Mask) >> Value2StartBit);
            }

            return kind;
        }

        public static ParserActionKind GetKind(int cell)
        {
            return (ParserActionKind)(cell & KindMask);
        }

        public static bool IsShift(int cell)
        {
            var kind = (ParserActionKind)(cell & KindMask);
            switch (kind)
            {
                case ParserActionKind.Shift:
                case ParserActionKind.ShiftReduce:
                    return true;
                default:
                    return false;
            }
        }

        public static int GetId(int cell)
        {
            return (cell & Value1Mask) >> Value1StartBit;
        }
    }
}
