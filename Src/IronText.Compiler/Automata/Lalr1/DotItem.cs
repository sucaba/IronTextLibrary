using IronText.Algorithm;
using IronText.Framework;
using System.Collections.Generic;
using IronText.Extensibility;
using IronText.Framework.Reflection;

namespace IronText.Automata.Lalr1
{
    public class DotItem : IParserDotItem
    {
        private readonly Production production;

        public DotItem(DotItem other)
            : this(other.production, other.Pos)
        {
        }

        public DotItem(Production production, int pos) 
        {
            this.production = production;
            this.Pos        = pos;
            this.Lookaheads = null;
        }

        public int Outcome { get { return production.OutcomeToken; } }

        public int this[int index]
        {
            get { return production.PatternTokens[index]; }
        }

        public int[] GetPattern() { return production.PatternTokens; }

        public bool IsAugmented { get { return production.IsAugmented; } }

        public int Size { get { return production.Size; } }

        public int Pos { get; private set; }

        public MutableIntSet Lookaheads { get; set; }

        public bool IsKernel
        {
            get { return Pos != 0 || production.IsAugmented; }
        }

        public int ProductionId { get { return production.Index; } }

        public bool IsReduce { get { return Pos == production.PatternTokens.Length; } }

        public bool IsShiftReduce { get { return (production.PatternTokens.Length - Pos) == 1; } }

        public int NextToken
        {
            get
            { 
                return Pos == production.PatternTokens.Length 
                     ? -1
                     : production.PatternTokens[Pos];
            }
        }

        public static bool operator ==(DotItem x, DotItem y) 
        { 
            return x.ProductionId == y.ProductionId && x.Pos == y.Pos; 
        }

        public static bool operator !=(DotItem x, DotItem y) 
        {
            return x.ProductionId != y.ProductionId || x.Pos != y.Pos; 
        }

        public override bool Equals(object obj) { return this == (DotItem)obj; }

        public override int GetHashCode() { return unchecked(production.Index + Pos); }

        public override string ToString()
        {
            return string.Format("(ProdId={0} Pos={1} LAs={2})", production.Index, Pos, Lookaheads);
        }

        public DotItem CreateNext()
        {
            return new DotItem(production, Pos + 1)
                        {
                            Lookaheads = Lookaheads.EditCopy()
                        };
        }

        Production IParserDotItem.Rule { get { return production; } }

        int IParserDotItem.Position { get { return Pos; } }

        IEnumerable<int> IParserDotItem.Lookaheads { get { return Lookaheads; } }
    }
}
