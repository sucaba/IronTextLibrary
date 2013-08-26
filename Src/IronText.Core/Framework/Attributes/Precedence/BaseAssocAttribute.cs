using System;
using System.Collections.Generic;
using IronText.Extensibility;

namespace IronText.Framework
{
    public abstract class BaseAssocAttribute : LanguageMetadataAttribute
    {
        public BaseAssocAttribute(int value, string term)
        {
            this.TermText = term;
            this.TermType = null;
            this.PrecedenceValue = value;
        }

        public BaseAssocAttribute(int value, Type term)
        {
            this.TermText = null;
            this.TermType = term;
            this.PrecedenceValue = value;
        }

        public string TermText { get; private set; }
        public Type TermType { get; private set; }
        public int PrecedenceValue { get; private set; }

        public override IEnumerable<KeyValuePair<TokenRef,Precedence>> GetTokenPrecedence(ITokenPool tokenPool)
        {
            TokenRef token;
            if (TermText == null)
            {
                token = tokenPool.GetToken(TermType);
            }
            else
            {
                token = tokenPool.GetLiteral(TermText);
            }

            yield return new KeyValuePair<TokenRef,Precedence>(
                token,
                new Precedence(PrecedenceValue, GetAssoc()));
        }

        protected abstract Associativity GetAssoc();
    }
}
