using IronText.Extensibility;
using IronText.Reflection.Managed;
using IronText.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public override IEnumerable<CilSymbolRef> GetSymbolsInCategory(SymbolCategory category)
        {
            if ((category & SymbolCategory.ExplicitlyUsed) != SymbolCategory.ExplicitlyUsed)
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
