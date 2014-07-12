using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IronText.Collections;
using IronText.Algorithm;
using IronText.Misc;
using IronText.Reflection.Utils;

namespace IronText.Reflection
{
    [DebuggerDisplay("{DebugProductionText}")]
    public sealed class Production : IndexableObject<IGrammarScope>, IProductionComponent
    {
        private readonly object _identity;

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
            OutcomeToken  = outcome.Index;
            Components    = components.ToArray();
            ContextRef    = contextRef ?? SemanticRef.None;
            Flags         = flags;

            Pattern       = CreateInputPattern(components);

            PatternTokens = Array.ConvertAll(Pattern, s => s.Index);

            this.Joint = new Joint();

            this._identity = BuildIdentity();
        }

        public int                OutcomeToken   { get; private set; }

        public int[]              PatternTokens  { get; private set; }

        public IProductionComponent[] Components { get; private set; }

        public Symbol             Outcome        { get; private set; }

        public Symbol[]           Pattern        { get; private set; }

        public Precedence         ExplicitPrecedence { get; set; }

        public int                Size           { get { return PatternTokens.Length; } }

        public bool               IsStart        { get { return Scope.Start == Outcome; } }

        public bool               IsAugmented    { get { return PredefinedTokens.AugmentedStart == OutcomeToken; } }

        public bool               IsExtended     { get { return Components.Any(c => c is Production); } }

        public Joint              Joint          { get; private set; }

        public SemanticRef        ContextRef     { get; private set; }

        public ProductionFlags    Flags          { get; private set; }

        public bool               IsDeleted      { get; private set; }

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

                int index = Array.FindLastIndex(Pattern, s => s.IsTerminal);
                return index < 0 ? null : Pattern[index].Precedence;
            }
        }

        public string Describe(Grammar grammar, int pos)
        {
            using (var writer = new StringWriter())
            {
                Describe(pos, writer);
                return writer.ToString();
            }
        }

        public string Describe(Grammar grammar)
        {
            using (var writer = new StringWriter())
            {
                Describe(grammar, writer);
                return writer.ToString();
            }
        }

        public void Describe(int pos, TextWriter output)
        {
            output.Write("{0} ->", Outcome.Name);

            int i = 0;
            foreach (var symbol in Pattern)
            {
                if (pos == i)
                {
                    output.Write(" •");
                }

                output.Write(" ");
                output.Write(symbol.Name);
                ++i;
            }

            if (pos == PatternTokens.Length)
            {
                output.Write(" •");
            }
        }

        public void Describe(Grammar grammar, TextWriter output)
        {
            output.Write("{0} ->", Outcome.Name);

            foreach (var symbol in Pattern)
            {
                output.Write(" ");
                output.Write(symbol.Name);
            }
        }

        protected override void DoAttached()
        {
            base.DoAttached();

            Outcome.Productions.Add(this);
        }

        protected override void DoDetaching()
        {
            Outcome.Productions.Remove(this);

            base.DoDetaching();
        }

        internal void SetAt(int pattIndex, Symbol symbol)
        {
            if (symbol == null)
            {
                PatternTokens[pattIndex]  = -1;
                Pattern[pattIndex] = null;
            }
            else
            {
                PatternTokens[pattIndex]  = symbol.Index;
                Pattern[pattIndex] = symbol;
            }
        }

        public string DebugProductionText
        {
            get
            {
                var output = new StringBuilder();
                if (IsDetached)
                {                
                    output
                        .Append(Outcome.Index)
                        .Append(" ->")
                        .Append(string.Join(" ", PatternTokens));
                }
                else
                {
                    output
                        .Append(Outcome.Name)
                        .Append(" ->");
                    if (Size == 0)
                    {
                        output.Append(" /*empty*/");
                    }
                    else
                    {
                        foreach (var component in Components)
                        {
                            var asSymbol = component as Symbol;
                            if (asSymbol != null)
                            {
                                output.Append(" ").Append(asSymbol.Name);
                            }
                            else
                            {
                                var prod = (Production)component;
                                output.Append(" (").Append(prod.DebugProductionText).Append(")");
                            }
                        }
                    }
                }

                return output.ToString();
            }
        }

        int IProductionComponent.Size
        {
            get { return Pattern.Length; }
        }

        void IProductionComponent.CopyTo(Symbol[] output, int startIndex)
        {
            int count = Pattern.Length;
            for (int i = 0; i != count; ++i)
            {
                output[startIndex++] = Pattern[i];
            }
        }

        private static Symbol[] CreateInputPattern(IEnumerable<IProductionComponent> components)
        {
            int size = components.Sum(comp => comp.Size);
            var pattern = new Symbol[size];
            int i = 0;
            foreach (var comp in components)
            {
                comp.CopyTo(pattern, i);
                i += comp.Size;
            }

            return pattern;
        }

        public void MarkDeleted()
        {
            this.IsDeleted = true;
            this.Outcome.Productions.Remove(this);
        }

        internal bool EqualTo(ProductionSketch sketch)
        {
            if (sketch == null || sketch.Outcome != Outcome.Name)
            {
                return false;
            }

            return Enumerable.Zip(Components, sketch.Components, ComponentEqualTo).All(s => s);
        }

        internal static bool ComponentEqualTo(IProductionComponent component, object sketchComp)
        {
            Symbol     symbol;
            Production prod;
            switch (component.Match(out symbol, out prod))
            {
                case 0: return symbol.Name == (sketchComp as string); 
                case 1: return prod.EqualTo(sketchComp as ProductionSketch);
                default:
                    throw new ArgumentException("component");
            }
        }

        internal ProductionSketch ToSketch()
        {
            return new ProductionSketch(Outcome.Name, Components.Select(ToSketch).ToArray());
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
            return IdentityFactory.FromIdentities(Outcome, Pattern);
        }
    }
}
