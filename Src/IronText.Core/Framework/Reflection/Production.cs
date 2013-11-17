using System;
using System.IO;
using System.Linq;

namespace IronText.Framework.Reflection
{
    [Serializable]
    public sealed class Production
    {
        public int        Id         { get; set; }

        public int        Outcome    { get; set; }

        public int[]      Pattern    { get; set; }

        public Precedence Precedence { get; private set; }

        public bool AssignPrecedence(Precedence value)
        {
            if (value != null)
            {
                var existingPrecedence = this.Precedence;
                if (existingPrecedence != null)
                {
                    if (!object.Equals(value, existingPrecedence))
                    {
                        return false;
                    }
                }
                else
                {
                    this.Precedence = value;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Production);
        }

        public bool Equals(Production other)
        {
            return other != null
                && other.Id == Id
                && other.Outcome == Outcome
                && Enumerable.SequenceEqual(other.Pattern, Pattern)
                ;
        }

        public override int GetHashCode()
        {
            unchecked 
            {
                return Id + Outcome + Pattern.Sum();
            }
        }

        public string Describe(EbnfGrammar grammar, int pos)
        {
            using (var writer = new StringWriter())
            {
                Describe(grammar, pos, writer);
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

        public void Describe(EbnfGrammar grammar, int pos, TextWriter output)
        {
            output.Write("{0} ->", grammar.SymbolName(Outcome));

            for (int i = 0; i != Pattern.Length; ++i)
            {
                if (pos == i)
                {
                    output.Write(" •");
                }

                output.Write(" ");
                output.Write(grammar.SymbolName(Pattern[i]));
            }

            if (pos == Pattern.Length)
            {
                output.Write(" •");
            }
        }

        public void Describe(EbnfGrammar grammar, TextWriter output)
        {
            output.Write("{0} ->", grammar.SymbolName(Outcome));

            for (int i = 0; i != Pattern.Length; ++i)
            {
                output.Write(" ");
                output.Write(grammar.SymbolName(Pattern[i]));
            }
        }
    }
}
