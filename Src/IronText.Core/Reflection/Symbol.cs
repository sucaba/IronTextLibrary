using IronText.Algorithm;
using IronText.Collections;
using IronText.Misc;
using IronText.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IronText.Reflection
{
    /// <summary>
    /// Deterministic symbol
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Name = {Name}")]
    public class Symbol : IndexableObject<IGrammarScope>, IProductionComponent, ITerminal
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

        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; protected set; }

        public bool IsPredefined { get; internal set; }

        public SymbolCategory Categories { get; set; }

        public bool IsAugmentedStart { get { return Scope.AugmentedProduction.Outcome == (object)this; } }

        public bool IsStart { get { return Scope.Start == (object)this; } }

        /// <summary>
        /// Determines whether symbol is terminal
        /// </summary>
        public bool IsTerminal { get { return Productions.Count == 0; } }

        /// <summary>
        /// Determines if symbol is input for at least some 'sequential' productions.
        /// </summary>
        public bool HasSideEffects
        {
            get
            {
                return this.Productions.Any(p => p.HasSideEffects);
            }
        }

        public ReferenceCollection<Production> Productions { get { return _productions; } }

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
        public Precedence Precedence { get; set; }

        int IProductionComponent.InputSize
        {
            get { return 1; }
        }

        void IProductionComponent.FillInput(Symbol[] output, int startIndex)
        {
            output[startIndex] = this;
        }

        IProductionComponent[] IProductionComponent.ChildComponents
        {
            get { return new IProductionComponent[0]; }
        }

        public bool IsUsed
        {
            get
            {
                return IsAugmentedStart
                    || IsStart
                    || Scope.Productions.Any(p => p.Input.Contains(this));
            }
        }

        public bool IsRecursive
        {
            get
            {
                Func<Symbol, IEnumerable<Symbol>> getChildren =
                    parent => parent.Productions.SelectMany(p => p.Input);

                var path = Graph.BreadthFirstSearch(
                                getChildren(this),
                                getChildren,
                                this.Equals);

                return path != null;
            }
        }

        protected override object DoGetIdentity()
        {
            return IdentityFactory.FromString(Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
