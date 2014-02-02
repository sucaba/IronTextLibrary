using System;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ProductionContext : IndexableObject<ISharedGrammarEntities>
    {
        public static readonly ProductionContext Global = new ProductionContext("$global");

        public ProductionContext(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.Name  = name;
            this.Joint = new Joint();
        }

        public string Name  { get; private set; }

        public Joint  Joint { get; private set; }

        public override bool Equals(object obj)
        {
            var casted = obj as ProductionContext;
            return Equals(casted);
        }

        public bool Equals(ProductionContext other)
        {
            return other != null && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString() { return Name; }
    }
}
