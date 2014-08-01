using IronText.Algorithm;
using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Reflection
{
    /// <summary>
    /// Deterministic symbol
    /// </summary>
    [Serializable]
    public class Symbol : SymbolBase, IProductionComponent, ITerminal
    {
        private readonly ReferenceCollection<Production> _productions;

        [NonSerialized]
        private readonly Joint _joint = new Joint();

        public Symbol(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.Name          = name;
            this._productions  = new ReferenceCollection<Production>();
            this.LocalScope    = new SemanticScope();
        }

        public new int Index { get {  return base.Index; } }

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

        /// <summary>
        /// Determines if symbol is input for at least some 'sequential' productions.
        /// </summary>
        public bool HasSideEffects
        {
            get
            {
                return this.Productions.Any(p => p.HasSideEffects && !p.IsHidden);
            }
        }

        public override ReferenceCollection<Production> Productions { get { return _productions; } }

        public Joint Joint { get { return _joint; } }

        /// <summary>
        /// Provided local context
        /// </summary>
        public SemanticScope LocalScope { get; private set; }

        /// <summary>
        /// Determines symbol-level precedence
        /// </summary>
        /// <remarks>
        /// If production has no associated precedence, it is calculated from
        /// the last terminal token in a production pattern.
        /// </remarks>
        public override Precedence Precedence { get; set; }

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

        public bool IsUsed
        {
            get
            {
                return IsAugmentedStart
                    || IsStart
                    || Scope.Productions.Any(p => !p.IsHidden && p.Pattern.Contains(this));
            }
        }

        public bool IsRecursive
        {
            get
            {
                Func<Symbol, IEnumerable<Symbol>> getChildren =
                    parent => parent.Productions.SelectMany(p => p.Pattern);

                var path = Graph.BreadthFirstSearch(
                                getChildren(this),
                                getChildren,
                                this.Equals);

                return path != null;
            }
        }
    }
}
