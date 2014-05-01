using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public interface IProductionComponent
    {
        int  Size { get; }

        void CopyTo(Symbol[] output, int startIndex);

        IProductionComponent[] Components { get; }

        void Accept(IProductionComponentVisitor visitor);
        T Accept<T>(IProductionComponentVisitor<T> visitor);
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
