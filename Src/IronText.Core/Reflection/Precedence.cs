﻿using System;

namespace IronText.Reflection
{
    public sealed class Precedence
    {
        public const int Min = -1;
        public const int Max = int.MaxValue;

        public Precedence(int value, Associativity assoc)
        {
            this.Value = value;
            this.Assoc = assoc;
        }

        public int Value { get; private set; }

        public Associativity Assoc { get; private set; }

        public static bool IsReduce(Precedence rulePrecedence, Precedence shiftPrecedence)
        {
            if (rulePrecedence == null)
            {
                throw new ArgumentNullException("rulePrecedence");
            }

            if (shiftPrecedence == null)
            {
                throw new ArgumentNullException("shiftPrecedence");
            }

            if (rulePrecedence.Value == shiftPrecedence.Value)
            {
                return rulePrecedence.Assoc == Associativity.Left;
            }

            return rulePrecedence.Value > shiftPrecedence.Value;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as Precedence;
            return casted != null
                && casted.Value == Value
                && casted.Assoc == Assoc
                ;
        }

        public override int GetHashCode()
        {
            unchecked { return Value + (int)Assoc; }
        }

        public static Precedence Default { get; set; }
    }
}
