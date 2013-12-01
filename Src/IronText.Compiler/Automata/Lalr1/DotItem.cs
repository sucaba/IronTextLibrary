using IronText.Algorithm;
using IronText.Framework;
using System.Collections.Generic;
using IronText.Extensibility;
using IronText.Framework.Reflection;

namespace IronText.Automata.Lalr1
{
    public class DotItem : IParserDotItem
    {
        public DotItem(Production production, int pos) 
        {
            this.Production = production;
            this.Pos        = pos;
            this.Lookaheads = null;
        }

        public Production Production { get; private set; }

        public int Pos { get; private set; }

        public MutableIntSet Lookaheads { get; set; }

        public bool IsKernel
        {
            get { return Pos != 0 || Production.IsAugmented; }
        }

        public int ProductionId { get { return Production.Index; } }

        public bool IsReduce { get { return Pos == Production.PatternTokens.Length; } }

        public bool IsShiftReduce { get { return (Production.PatternTokens.Length - Pos) == 1; } }

        public int NextToken { get { return Production.PatternTokens[Pos]; } }

        public static bool operator ==(DotItem x, DotItem y) 
        { 
            return x.ProductionId == y.ProductionId && x.Pos == y.Pos; 
        }

        public static bool operator !=(DotItem x, DotItem y) 
        {
            return x.ProductionId != y.ProductionId || x.Pos != y.Pos; 
        }

        public override bool Equals(object obj) { return this == (DotItem)obj; }

        public override int GetHashCode() { return unchecked(Production.Index + Pos); }

        public override string ToString()
        {
            return string.Format("(ProdId={0} Pos={1} LAs={2})", Production.Index, Pos, Lookaheads);
        }

        Production IParserDotItem.Rule { get { return Production; } }

        int IParserDotItem.Position { get { return Pos; } }

        IEnumerable<int> IParserDotItem.Lookaheads { get { return Lookaheads; } }
    }
}
