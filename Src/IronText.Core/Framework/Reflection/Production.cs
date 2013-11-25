using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public sealed class Production : IndexableObject<IEbnfContext>
    {
        public Production()
        {
            Actions = new ReferenceCollection<ProductionAction>();
        }

        public int    Outcome    { get; set; }

        public int[]  Pattern    { get; set; }

        public bool IsStart { get { return Context.StartToken == Outcome; } }

        public Symbol OutcomeSymbol
        {
            get { return (Symbol)Context.Symbols[Outcome]; }
        }

        public IEnumerable<Symbol> PatternSymbols
        {
            get
            {
                var symbols = Context.Symbols;
                return (from t in Pattern select (Symbol)symbols[t]);
            }
        }

        public Precedence ExplicitPrecedence { get; set; }

        public Precedence EffectivePrecedence
        {
            get
            {
                if (ExplicitPrecedence != null)
                {
                    return ExplicitPrecedence;
                }

                int index = Array.FindLastIndex(Pattern, t => Context.Symbols[t].IsTerminal);
                return index < 0 ? null : Context.Symbols[Pattern[index]].Precedence;
            }
        }

        /// <summary>
        /// Semantic actions for the production
        /// </summary>
        /// <remarks>
        /// Typically production contains single action, however
        /// when production is inlined there are multiple actions
        /// happing when this production being applied.
        /// </remarks>
        public ReferenceCollection<ProductionAction> Actions { get; private set; }

        public bool Equals(Production other)
        {
            return other != null
                && other.Index == Index
                && other.Outcome == Outcome
                && Enumerable.SequenceEqual(other.Pattern, Pattern)
                ;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Production);
        }

        public override int GetHashCode()
        {
            return unchecked(Outcome + Pattern.Sum());
        }

        public string Describe(EbnfGrammar grammar, int pos)
        {
            using (var writer = new StringWriter())
            {
                Describe(pos, writer);
                return writer.ToString();
            }
        }

        public string Describe(EbnfGrammar grammar)
        {
            using (var writer = new StringWriter())
            {
                Describe(grammar, writer);
                return writer.ToString();
            }
        }

        public void Describe(int pos, TextWriter output)
        {
            output.Write("{0} ->", OutcomeSymbol.Name);

            int i = 0;
            foreach (var symbol in PatternSymbols)
            {
                if (pos == i)
                {
                    output.Write(" •");
                }

                output.Write(" ");
                output.Write(symbol.Name);
                ++i;
            }

            if (pos == Pattern.Length)
            {
                output.Write(" •");
            }
        }

        public void Describe(EbnfGrammar grammar, TextWriter output)
        {
            output.Write("{0} ->", OutcomeSymbol.Name);

            foreach (var symbol in PatternSymbols)
            {
                output.Write(" ");
                output.Write(symbol.Name);
            }
        }

        protected override void DoAttached()
        {
            base.DoAttached();

            Context.Symbols[Outcome].Productions.Add(this);
        }

        protected override void DoDetaching()
        {
            Context.Symbols[Outcome].Productions.Remove(this);

            base.DoDetaching();
        }
    }
}
