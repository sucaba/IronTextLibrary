using IronText.Algorithm;
using IronText.Runtime;

namespace IronText.Compiler.Analysis
{
    public class DotItem
    {
        private readonly RuntimeProduction production;

        public DotItem(DotItem other)
            : this(other.production, other.Position)
        {
        }

        internal DotItem(RuntimeProduction production, int pos)
        {
            this.production = production;
            this.Position   = pos;
            this.LA         = null;
        }

        public int Outcome { get { return production.Outcome; } }

        public int this[int index] { get { return production.Input[index]; } }

        public int[] GetInputTokens() { return production.Input; }

        public bool IsAugmented { get { return PredefinedTokens.AugmentedStart == production.Outcome; } }

        public int Size { get { return production.Input.Length; } }

        public int Position { get; private set; }

        public MutableIntSet LA { get; set; }

        public bool IsKernel
        {
            get { return Position != 0 || IsAugmented; }
        }

        public int ProductionId { get { return production.Index; } }

        public bool IsReduce { get { return Position == production.Input.Length; } }

        public bool IsShiftReduce { get { return (production.Input.Length - Position) == 1; } }

        public int NextToken
        {
            get
            { 
                return Position == production.Input.Length 
                     ? -1
                     : production.Input[Position];
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
    }
}
