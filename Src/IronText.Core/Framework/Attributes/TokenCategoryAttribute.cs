using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Reflection;
using IronText.Reflection.Managed;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple = true)]
    public class TokenCategoryAttribute : LanguageMetadataAttribute
    {
        private SymbolCategory categories;

        public TokenCategoryAttribute() { }

        public TokenCategoryAttribute(Type type, SymbolCategory category)
        {
            this.TokenType = type;
            this.categories = category;
        }

        public TokenCategoryAttribute(string text, SymbolCategory category)
        {
            this.Text = text;
            this.categories = category;
        }

        public Type TokenType { get; set; }

        public string Text { get; set; }

        public override IEnumerable<CilSymbolRef> GetSymbolsInCategory(SymbolCategory requestedCategories)
        {
            if ((requestedCategories & this.categories) == 0)
            {
                return Enumerable.Empty<CilSymbolRef>();
            }

            if (Text == null)
            {
                return new[] { CilSymbolRef.Create(TokenType) };
            }
            else
            {
                return new[] { CilSymbolRef.Create(Text) };
            }
        }
    }
}
