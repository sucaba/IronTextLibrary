using IronText.Collections;

namespace IronText.Reflection
{
    /// <summary>
    /// Deterministic symbol
    /// </summary>
    public class Symbol : SymbolBase, IProductionComponent
    {
        private readonly ReferenceCollection<Production> _productions;

        public Symbol(string name)
        {
            this.Name          = name ?? Grammar.UnnamedTokenName;
            this._productions  = new ReferenceCollection<Production>();
            this.LocalContextProvider = new SemanticScope();
            this.Joint         = new Joint();
        }

        public bool IsAugmentedStart { get { return PredefinedTokens.AugmentedStart == Index; } }

        public bool IsStart { get { return Scope.Start == this; } }

        /// <summary>
        /// Categories token belongs to
        /// </summary>
        public override SymbolCategory Categories { get; set; }

        /// <summary>
        /// Determines whether symbol is terminal
        /// </summary>
        public override bool IsTerminal { get { return Productions.Count == 0; } }

        public override ReferenceCollection<Production> Productions { get { return _productions; } }

        public Joint Joint { get; private set; }

        /// <summary>
        /// Provided local context
        /// </summary>
        public SemanticScope LocalContextProvider { get; private set; }

        /// <summary>
        /// Determines symbol-level precedence
        /// </summary>
        /// <remarks>
        /// If production has no associated precedence, it is calculated from
        /// the last terminal token in a production pattern.
        /// </remarks>
        public override Precedence Precedence { get; set; }

        public override bool Equals(object obj)
        {
            var casted = obj as Symbol;
            return casted != null && casted.Index == Index;
        }

        public override int GetHashCode() { return Index; }

        protected override SymbolBase DoClone()
        {
            return new Symbol(Name)
            {
                Precedence = Precedence,
                Categories = Categories
            };
        }

        int IProductionComponent.Size
        {
            get { return 1; }
        }

        void IProductionComponent.CopyTo(Symbol[] output, int startIndex)
        {
            output[startIndex] = this;
        }

        IProductionComponent[] IProductionComponent.Components
        {
            get { return new IProductionComponent[0]; }
        }

        T IProductionComponent.Accept<T>(IProductionComponentVisitor<T> visitor)
        {
            return visitor.VisitSymbol(this);
        }
    }
}
