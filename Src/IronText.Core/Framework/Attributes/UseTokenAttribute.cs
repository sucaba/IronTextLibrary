using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple = true)]
    public class UseTokenAttribute : LanguageMetadataAttribute
    {
        public UseTokenAttribute() { }

        public UseTokenAttribute(Type type)
        {
            this.TokenType = type;
        }

        public UseTokenAttribute(string text)
        {
            this.Text = text;
        }

        public Type TokenType { get; set; }

        public string Text { get; set; }

        public override IEnumerable<TokenRef> GetTokensInCategory(ITokenPool tokenPool, SymbolCategory category)
        {
            if ((category & SymbolCategory.ExplicitlyUsed) != SymbolCategory.ExplicitlyUsed)
            {
                return Enumerable.Empty<TokenRef>();
            }

            if (Text == null)
            {
                return new[] { tokenPool.GetToken(TokenType) };
            }
            else
            {
                return new[] { tokenPool.GetLiteral(Text) };
            }
        }
    }
}
