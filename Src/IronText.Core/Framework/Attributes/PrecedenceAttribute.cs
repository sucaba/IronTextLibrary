using System;
using System.Collections.Generic;
using IronText.Extensibility;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple=true)]
    public class PrecedenceAttribute : LanguageMetadataAttribute
    {
        public PrecedenceAttribute(string term, int value, Associativity assoc = Associativity.Left)
        {
            this.TermText = term;
            this.TermType = null;
            this.PrecedenceValue = value;
            this.Associativity = assoc;
        }

        public PrecedenceAttribute(Type term, int value, Associativity assoc = Associativity.Left)
        {
            this.TermText = null;
            this.TermType = term;
            this.PrecedenceValue = value;
            this.Associativity = assoc;
        }

        public string TermText { get; private set; }

        public Type TermType { get; private set; }

        public int PrecedenceValue { get; set; }

        public Associativity Associativity { get; set; }

        public override IEnumerable<CilSymbolFeature<Precedence>> GetSymbolPrecedence()
        {
            CilSymbolRef token;
            if (TermText == null)
            {
                token = CilSymbolRef.Typed(TermType);
            }
            else
            {
                token = CilSymbolRef.Literal(TermText);
            }

            yield return new CilSymbolFeature<Precedence>(
                token,
                new Precedence(PrecedenceValue, Associativity));
        }
    }
}
