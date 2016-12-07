using IronText.Collections;
using System;

namespace IronText.Runtime
{
    public class ParserDecision
        : Ambiguous<ParserDecision>
        , IEquatable<ParserDecision>
    {
        public ParserDecision(ParserInstruction instruction)
        {
            Instruction = instruction;
        }

        public ParserInstruction Instruction { get; }

        public bool Equals(ParserDecision other) =>
            other != null
            && Equals(Instruction, other.Instruction)
            && Alternative == (object)other.Alternative;

        public override bool Equals(object obj) => Equals(obj as ParserDecision);

        public override int GetHashCode() => Instruction.GetHashCode();
    }
}
