using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    [DebuggerDisplay("{DebugProductionText}")]
    public sealed class Production : IndexableObject<IEbnfContext>
    {
        private ProductionAction _action;
        
        public Production(Symbol outcome, IEnumerable<Symbol> pattern)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException("outcome");
            }

            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            Outcome  = outcome;
            OutcomeToken   = outcome.Index;
            Pattern = pattern.ToArray();
            PatternTokens  = Array.ConvertAll(Pattern, s => s == null ? -1 : s.Index);

            _action = new SimpleProductionAction(Size);
        }

        public int    OutcomeToken    { get; private set; }

        public int[]  PatternTokens    { get; private set; }

        public int Size { get { return PatternTokens.Length; } }

        public bool IsStart { get { return Context.Start == Outcome; } }

        public bool IsAugmented { get { return EbnfGrammar.AugmentedStart == OutcomeToken; } }

        public Symbol Outcome { get; private set; }

        public Symbol[] Pattern { get; private set; }

        public Precedence ExplicitPrecedence { get; set; }

        public Precedence EffectivePrecedence
        {
            get
            {
                if (ExplicitPrecedence != null)
                {
                    return ExplicitPrecedence;
                }

                int index = Array.FindLastIndex(PatternTokens, t => Context.Symbols[t].IsTerminal);
                return index < 0 ? null : Context.Symbols[PatternTokens[index]].Precedence;
            }
        }

        /// <summary>
        /// Semantic actions for the production.
        /// </summary>
        /// <remarks>
        /// Typically production contains single action, however
        /// when production is inlined there are multiple actions
        /// happing when this production being applied.
        /// </remarks>
        public ProductionAction Action
        {
            get { return _action; }
            set { _action = value ?? new SimpleProductionAction(Size); }
        }

        public bool Equals(Production other)
        {
            return other != null
                && other.Index == Index
                && other.OutcomeToken == OutcomeToken
                && Enumerable.SequenceEqual(other.PatternTokens, PatternTokens)
                ;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Production);
        }

        public override int GetHashCode()
        {
            return unchecked(OutcomeToken + PatternTokens.Sum());
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

        public void Describe(EbnfGrammar grammar, TextWriter output)
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
                        .Append(OutcomeToken)
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
    }
}
