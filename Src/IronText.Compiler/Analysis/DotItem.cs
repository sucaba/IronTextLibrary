using IronText.Algorithm;
using IronText.Framework;
using System.Collections.Generic;
using IronText.Extensibility;
using IronText.Framework.Reflection;

namespace IronText.Compiler.Analysis
{
    public class DotItem : IParserDotItem
    {
        private readonly Production production;

        public DotItem(DotItem other)
            : this(other.production, other.Position)
        {
        }

        public DotItem(Production production, int pos) 
        {
            this.production = production;
            this.Position        = pos;
            this.LA = null;
        }

        public int Outcome { get { return production.OutcomeToken; } }

        public int this[int index] { get { return production.PatternTokens[index]; } }

        public int[] GetPattern() { return production.PatternTokens; }

        public bool IsAugmented { get { return production.IsAugmented; } }

        public int Size { get { return production.Size; } }

        public int Position { get; private set; }

        public MutableIntSet LA { get; set; }

        public bool IsKernel
        {
            get { return Position != 0 || production.IsAugmented; }
        }

        public int ProductionId { get { return production.Index; } }

        public bool IsReduce { get { return Position == production.PatternTokens.Length; } }

        public bool IsShiftReduce { get { return (production.PatternTokens.Length - Position) == 1; } }

        public int NextToken
        {
            get
            { 
                return Position == production.PatternTokens.Length 
                     ? -1
                     : production.PatternTokens[Position];
            }
        }

        public static bool operator ==(DotItem x, DotItem y) 
        { 
            return x.ProductionId == y.ProductionId && x.Position == y.Position; 
        }

        public static bool operator !=(DotItem x, DotItem y) 
        {
            return x.ProductionId != y.ProductionId || x.Position != y.Position; 
        }

        public override bool Equals(object obj) { return this == (DotItem)obj; }

        public override int GetHashCode() { return unchecked(production.Index + Position); }

        public override string ToString()
        {
            return string.Format("(ProdId={0} Pos={1} LAs={2})", production.Index, Position, LA);
        }

        public DotItem CreateNextItem()
        {
            return new DotItem(production, Position + 1)
                        {
                            LA = LA.EditCopy()
                        };
        }

        Production IParserDotItem.Production { get { return production; } }

        int IParserDotItem.Position { get { return Position; } }

        IEnumerable<int> IParserDotItem.Lookaheads { get { return LA; } }
    }
}
