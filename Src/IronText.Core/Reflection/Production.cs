using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IronText.Collections;
using IronText.Algorithm;

namespace IronText.Reflection
{
    [DebuggerDisplay("{DebugProductionText}")]
    public sealed class Production : IndexableObject<ISharedGrammarEntities>, IProductionComponent
    {
        public Production(Symbol outcome, IEnumerable<IProductionComponent> components, SemanticContextRef contextRef)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException("outcome");
            }

            if (components == null)
            {
                throw new ArgumentNullException("inputs");
            }

            Outcome       = outcome;
            OutcomeToken  = outcome.Index;
            Components    = components.ToArray();
            ContextRef    = contextRef ?? SemanticContextRef.None;

            Pattern       = CreateInputPattern(components);

            PatternTokens = Array.ConvertAll(Pattern, s => s.Index);

            this.Joint = new Joint();
        }


        public int                OutcomeToken   { get; private set; }

        public int[]              PatternTokens  { get; private set; }

        public IProductionComponent[] Components { get; set; }

        public Symbol             Outcome        { get; private set; }

        public Symbol[]           Pattern        { get; private set; }

        public Precedence         ExplicitPrecedence { get; set; }

        public int                Size           { get { return PatternTokens.Length; } }

        public bool               IsStart        { get { return Scope.Start == Outcome; } }

        public bool               IsAugmented    { get { return PredefinedTokens.AugmentedStart == OutcomeToken; } }

        public Joint              Joint          { get; private set; }

        public SemanticContextRef ContextRef     { get; private set; }

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

        public bool Equals(Production other)
        {
            return other != null
                && other.Index == Index
                && other.Outcome.Index == Outcome.Index
                && Enumerable.SequenceEqual(other.Pattern, Pattern)
                ;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Production);
        }

        public override int GetHashCode()
        {
            return unchecked(Index + Outcome.Index + Pattern.Sum(s => s.Index));
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
                        foreach (var symbol in Pattern)
                        {
                            output.Append(" ").Append(symbol.Name);
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
    }
}
