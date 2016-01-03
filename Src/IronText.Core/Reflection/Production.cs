using IronText.Collections;
using IronText.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace IronText.Reflection
{
    [Serializable]
    [DebuggerDisplay("{DebugProductionText}")]
    public sealed class Production : IndexableObject<IGrammarScope>, IProductionComponent
    {
        private readonly object _identity;

        [NonSerialized]
        private readonly Joint _joint = new Joint();

        /// <summary>
        /// Create identity production
        /// </summary>
        /// <param name="outcome"></param>
        /// <param name="input"></param>
        public Production(Symbol outcome, Symbol input)
            : this(outcome, new [] { input })
        {
            this.HasIdentityAction = true;
        }

        public Production(
            Symbol           outcome,
            IEnumerable<IProductionComponent> components,
            SemanticRef      contextRef = null,
            ProductionFlags  flags = ProductionFlags.None)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException("outcome");
            }

            if (components == null)
            {
                throw new ArgumentNullException("components");
            }

            Outcome       = outcome;
            ChildComponents    = components.ToArray();
            ContextRef    = contextRef ?? SemanticRef.None;
            Flags         = flags;

            Input         = CreateInputPattern(components);

            this._identity = BuildIdentity();

            Semantics     = new ProductionSemantics(this);
        }

        public int                OutcomeToken   { get { return Outcome.Index; } }

        public IEnumerable<int>   InputTokens    { get { return Input.Select(s => s.Index); } }

        public IProductionComponent[] ChildComponents { get; private set; }

        public Symbol             Outcome        { get; private set; }

        public Symbol[]           Input          { get; private set; }

        public Precedence         ExplicitPrecedence { get; set; }

        public int                InputLength    { get { return Input.Length; } }

        public bool               IsStart        { get { return Scope.Start == Outcome; } }

        public bool               IsAugmented    { get { return this == (object)Scope.AugmentedProduction; } }

        public bool               IsExtended     { get { return ChildComponents.Any(c => c is Production); } }

        public Joint              Joint          { get { return _joint; } }

        public ProductionSemantics Semantics    { get; private set; }

        public SemanticRef        ContextRef     { get; private set; }

        public ProductionFlags    Flags          { get; private set; }

        public bool               HasIdentityAction { get; private set; }

        public bool               HasSideEffects 
        { 
            get 
            { 
                return (Flags & ProductionFlags.HasSideEffects) == ProductionFlags.HasSideEffects;
            }
        }

        public bool               IsUsed
        {
            get { return Outcome.IsUsed; }
        }

        public Precedence EffectivePrecedence
        {
            get
            {
                if (ExplicitPrecedence != null)
                {
                    return ExplicitPrecedence;
                }

                int index = Array.FindLastIndex(Input, s => s.IsTerminal);
                return index < 0 ? null : Input[index].Precedence;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            Outcome.Productions.Add(this);
        }

        protected override void OnDetaching()
        {
            Outcome.Productions.Remove(this);

            base.OnDetaching();
        }

        internal void SetAt(int pattIndex, Symbol symbol)
        {
            if (symbol == null)
            {
                ChildComponents[0] = null;
                Input[0]      = null;
            }
            else
            {
                ChildComponents[pattIndex] = symbol;
                Input[0]              = symbol;
            }
        }

        int IProductionComponent.InputSize
        {
            get { return Input.Length; }
        }

        void IProductionComponent.FillInput(Symbol[] output, int startIndex)
        {
            int count = Input.Length;
            for (int i = 0; i != count; ++i)
            {
                output[startIndex++] = Input[i];
            }
        }

        private static Symbol[] CreateInputPattern(IEnumerable<IProductionComponent> components)
        {
            int size = components.Sum(comp => comp.InputSize);
            var pattern = new Symbol[size];
            int i = 0;
            foreach (var comp in components)
            {
                comp.FillInput(pattern, i);
                i += comp.InputSize;
            }

            return pattern;
        }

        internal ProductionSketch ToSketch()
        {
            return new ProductionSketch(Outcome.Name, ChildComponents.Select(ToSketch).ToArray());
        }

        internal static object ToSketch(IProductionComponent component)
        {
            Symbol     symbol;
            Production prod;
            switch (component.Match(out symbol, out prod))
            {
                case 0: return symbol.Name; 
                case 1: return prod.ToSketch();
                default:
                    throw new ArgumentException("component");
            }
        }

        protected override object DoGetIdentity()
        {
            return _identity;
        }

        private object BuildIdentity()
        {
            // Production identity is based on output and input signature and not on 
            // on the internal components i.e. production signature should be unique
            return IdentityFactory.FromIdentities(Outcome, Input);
        }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                Describe(writer);
                return writer.ToString();
            }
        }

        public void Describe(TextWriter output)
        {
            output.Write("{0} =", Outcome.Name);

            foreach (var symbol in Input)
            {
                output.Write(" ");
                output.Write(symbol.Name);
            }
        }

        public string DebugProductionText { get { return ToString(); } }
    }
}
