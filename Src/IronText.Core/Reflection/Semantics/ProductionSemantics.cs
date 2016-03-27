﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronText.Reflection
{
    [Serializable]
    public class ProductionSemantics
        : IEnumerable<SemanticFormula>
        , IProductionSemanticScope
    {
        private readonly Production prod;
        private List<SemanticFormula> formulas = new List<SemanticFormula>();
        public ProductionSemantics(Production prod)
        {
            this.prod = prod;
        }

        private void Add(SemanticFormula formula)
        {
            formulas.Add(formula);
            ((IProductionSemanticElement)formula).Attach(this);

            ResolveVariable(formula.Lhe);
        }

        private ISymbolProperty ResolveVariable(SemanticVariable lhe)
        {
            ISymbolProperty result;

            Symbol symbol = lhe.ResolveSymbol();
            if (lhe.Position < 0)
            {
                result = prod.Scope.SymbolProperties.FindOrAdd(symbol, lhe.Name);
            }
            else
            {
                result = prod.Scope.InheritedProperties.FindOrAdd(symbol, lhe.Name);
            }

            return result;
        }

        public void Add(
            SemanticVariable      lhe,
            SemanticReference     rhe)
        {
            Add(new SemanticFormula(lhe, rhe));
        }

        public void Add(
            SemanticVariable lhe,
            SemanticConstant rhe)
        {
            Add(new SemanticFormula(lhe, rhe));
        }

        public void Add<T>(
            SemanticVariable      lhe,
            SemanticReference[]   actualRefs,
            Expression<Func<T,T>> func)
        {
            Add(new SemanticFormula(lhe, actualRefs, func));
        }

        public void Add<T1, T2, T3, T4, T>(
            SemanticVariable lhe,
            SemanticReference[] actualRefs,
            Expression<Func<T1, T2, T3, T4, T>> func)
        {
            Add(new SemanticFormula(lhe, actualRefs, func));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return formulas.GetEnumerator();
        }

        IEnumerator<SemanticFormula> IEnumerable<SemanticFormula>.GetEnumerator()
        {
            return formulas.GetEnumerator();
        }

        ISymbolProperty IProductionSemanticScope.ResolveProperty(Symbol symbol, string name, bool isInherited)
        {
            var scope = prod.Scope;
            scope.RequireImmutable();

            ISymbolProperty result;
            if (isInherited)
            {
                result = scope.InheritedProperties.Find(symbol, name);
            }
            else
            {
                result = scope.SymbolProperties.Find(symbol, name);
            }

            if (result == null)
            {
                throw new ArgumentException($"Property {name} does not exist", nameof(name));
            }

            return result;
        }

        Symbol IProductionSemanticScope.Outcome { get { return prod.Outcome; } }

        Symbol[] IProductionSemanticScope.Input { get { return prod.Input; } }

    }
}
