using IronText.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace IronText.Runtime
{
    public class ParserDecision
        : Ambiguous<ParserDecision>
        , IEquatable<ParserDecision>
    {
        public ParserDecision()
        {
        }

        public ParserDecision(ParserInstruction instruction)
        {
            Instructions.Add(instruction);
        }

        public List<ParserInstruction> Instructions { get; } = new List<ParserInstruction>();

        public bool Equals(ParserDecision other) =>
            other != null
            && Alternative == other.Alternative
            && Enumerable.SequenceEqual(Instructions, other.Instructions);
    }
}
