using System;
using System.IO;
using System.Linq;

namespace IronText.Framework
{
    [Serializable]
    public class BnfRule
    {
        public int      Id;
        public int      Left;
        public int[]    Parts;
        public Precedence Precedence;

        public override bool Equals(object obj)
        {
            var casted = obj as BnfRule;
            return casted != null
                && casted.Id == Id
                && casted.Left == Left
                && Enumerable.SequenceEqual(casted.Parts, Parts)
                ;
        }

        public override int GetHashCode()
        {
            unchecked 
            {
                return Id + Left + Parts.Sum();
            }
        }

        public string Describe(BnfGrammar grammar, int pos)
        {
            using (var writer = new StringWriter())
            {
                Describe(grammar, pos, writer);
                return writer.ToString();
            }
        }

        public string Describe(BnfGrammar grammar)
        {
            using (var writer = new StringWriter())
            {
                Describe(grammar, writer);
                return writer.ToString();
            }
        }

        public void Describe(BnfGrammar grammar, int pos, TextWriter output)
        {
            output.Write("{0} ->", grammar.TokenName(Left));

            for (int i = 0; i != Parts.Length; ++i)
            {
                if (pos == i)
                {
                    output.Write(" •");
                }

                output.Write(" ");
                output.Write(grammar.TokenName(Parts[i]));
            }

            if (pos == Parts.Length)
            {
                output.Write(" •");
            }
        }

        public void Describe(BnfGrammar grammar, TextWriter output)
        {
            output.Write("{0} ->", grammar.TokenName(Left));

            for (int i = 0; i != Parts.Length; ++i)
            {
                output.Write(" ");
                output.Write(grammar.TokenName(Parts[i]));
            }
        }
    }
}
