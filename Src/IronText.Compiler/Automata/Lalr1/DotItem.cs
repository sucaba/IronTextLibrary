using IronText.Algorithm;
using IronText.Framework;
using System.Collections.Generic;
using IronText.Extensibility;

namespace IronText.Automata.Lalr1
{
    public class DotItem : IParserDotItem
    {
        public readonly BnfRule Rule;
        public readonly int Pos;
        public MutableIntSet Lookaheads;

        public DotItem(BnfRule rule, int pos) 
        {
            this.Rule = rule;
            this.Pos = pos;
            this.Lookaheads = null;
        }

        public bool IsKernel
        {
            get { return Pos != 0 || Rule.Left == BnfGrammar.AugmentedStart; }
        }

        public int RuleId { get { return Rule.Id; } }

        public bool IsReduce { get { return Pos == Rule.Parts.Length; } }

        public bool IsShiftReduce { get { return (Rule.Parts.Length - Pos) == 1; } }

        public int NextToken { get { return Rule.Parts[Pos]; } }

        public static bool operator ==(DotItem x, DotItem y) 
        { 
            return x.RuleId == y.RuleId && x.Pos == y.Pos; 
        }

        public static bool operator !=(DotItem x, DotItem y) 
        {
            return x.RuleId != y.RuleId || x.Pos != y.Pos; 
        }

        public override bool Equals(object obj) { return this == (DotItem)obj; }

        public override int GetHashCode() { unchecked { return Rule.Id + Pos; } }

        public override string ToString()
        {
            return string.Format("(Rule={0} Pos={1} LAs={2})", Rule.Id, Pos, Lookaheads);
        }

        BnfRule IParserDotItem.Rule { get { return Rule; } }

        int IParserDotItem.Position { get { return Pos; } }


        IEnumerable<int> IParserDotItem.Lookaheads
        {
            get { return Lookaheads; }
        }
    }
}
