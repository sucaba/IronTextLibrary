using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public class Symbol
    {
        public int Id;
        public string Name;       // Display name
        public TokenCategory Categories;
        public bool IsTerm;
        public Precedence Precedence;
        public readonly List<Production> Productions = new List<Production>();

        public override bool Equals(object obj)
        {
            var casted = obj as Symbol;
            return casted != null
                && casted.Name == Name
                && casted.Categories == Categories
                && object.Equals(casted.Precedence, Precedence)
                ;
        }

        public override int GetHashCode()
        {
            int result = 0;
            unchecked
            {
                if (Name != null)
                {
                    result += Name.GetHashCode();
                }

                result += Categories.GetHashCode();
                if (Precedence != null)
                {
                    result += Precedence.GetHashCode();
                }
            }

            return result;
        }
    }
}
