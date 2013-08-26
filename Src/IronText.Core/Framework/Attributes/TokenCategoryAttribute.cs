using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple = true)]
    public class TokenCategoryAttribute : LanguageMetadataAttribute
    {
        private TokenCategory categories;

        public TokenCategoryAttribute() { }

        public TokenCategoryAttribute(Type type, TokenCategory category)
        {
            this.TokenType = type;
            this.categories = category;
        }

        public TokenCategoryAttribute(string text, TokenCategory category)
        {
            this.Text = text;
            this.categories = category;
        }

        public Type TokenType { get; set; }

        public string Text { get; set; }

        public override IEnumerable<TokenRef> GetTokensInCategory(ITokenPool tokenPool, TokenCategory requestedCategories)
        {
            if ((requestedCategories & this.categories) == 0)
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
