using IronText.Misc;
using System;

namespace IronText.Reflection
{
    public interface IProductionComponent : IDiscriminatable, IHasIdentity
    {
        int  InputSize { get; }

        void FillInput(Symbol[] input, int startIndex);

        IProductionComponent[] ChildComponents { get; }
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
            InjectedActionParameter param;
            switch (self.Match(out symbol, out production, out param))
            {
                case 0: return visitor.Visit(symbol);
                case 1: return visitor.Visit(production);
                case 2: return visitor.Visit(param);
            }

            throw new ArgumentException("self");
        }
    }

    public interface IProductionComponentVisitor
    {
        void Visit(Symbol symbol);

        void Visit(Production production);

        void Visit(InjectedActionParameter param);
    }

    public interface IProductionComponentVisitor<T>
    {
        T Visit(Symbol symbol);

        T Visit(Production production);

        T Visit(InjectedActionParameter param);
    }
}
