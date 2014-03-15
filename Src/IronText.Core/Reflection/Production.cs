using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    [DebuggerDisplay("{DebugProductionText}")]
    public sealed class Production : IndexableObject<ISharedGrammarEntities>
    {
        private readonly ForeignActionSequence _actions;
        
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

            Outcome       = outcome;
            OutcomeToken  = outcome.Index;
            Pattern       = pattern.ToArray();
            PatternTokens = Array.ConvertAll(Pattern, s => s == null ? -1 : s.Index);

            _actions = new ForeignActionSequence();
        }

        public int               OutcomeToken   { get; private set; }

        public int[]             PatternTokens  { get; private set; }

        public Symbol            Outcome        { get; private set; }

        public Symbol[]          Pattern        { get; private set; }

        public Precedence        ExplicitPrecedence { get; set; }

        public int  Size        { get { return PatternTokens.Length; } }

        public bool IsStart     { get { return Scope.Start == Outcome; } }

        public bool IsAugmented { get { return PredefinedTokens.AugmentedStart == OutcomeToken; } }

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

        /// <summary>
        /// Semantic actions for the production.
        /// </summary>
        /// <remarks>
        /// Typically production contains single action, however
        /// when production is inlined there are multiple actions
        /// happing when this production being applied.
        /// </remarks>
        public ForeignActionSequence Actions
        {
            get { return _actions; }
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
    }
}
