using IronText.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public interface IProductionComponent : IDiscriminatable
    {
        int  Size { get; }

        void CopyTo(Symbol[] output, int startIndex);

        IProductionComponent[] Components { get; }
    }
    
    public static class ProductionComponentVisitorExtensions
    {
        public static void Accept(this IProductionComponent self, IProductionComponentVisitor visitor)
        {
            Symbol     symbol;
            Production production;
            if (self.Match(out symbol, out production) != 1)
            {
                return;
            }
        }

        public static T Accept<T>(this IProductionComponent self, IProductionComponentVisitor<T> visitor)
        {
            Symbol     symbol;
            Production production;
            switch (self.Match(out symbol, out production))
            {
                case 0: return visitor.VisitSymbol(symbol);
                case 1: return visitor.VisitProduction(production);
            }

            throw new ArgumentException("self");
        }
    }

    public interface IProductionComponentVisitor
    {
        void VisitSymbol(Symbol symbol);

        void VisitProduction(Production production);
    }

    public interface IProductionComponentVisitor<T>
    {
        T VisitSymbol(Symbol symbol);

        T VisitProduction(Production production);
    }
}
