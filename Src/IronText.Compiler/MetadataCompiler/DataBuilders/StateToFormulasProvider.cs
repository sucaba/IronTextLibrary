﻿using IronText.Automata.Lalr1;
using IronText.Reflection;
using IronText.Runtime.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Compiler.Analysis;

namespace IronText.MetadataCompiler
{
    internal interface IStateToFormulasProvider
    {
        RuntimeFormula[][] GetData();
    }

    internal class StateToFormulasProvider : IStateToFormulasProvider
    {
        private readonly RuntimeFormula[][] data;
        private readonly Grammar grammar;

        public StateToFormulasProvider(Grammar grammar, ILrDfa dfa)
        {
            this.grammar = grammar;
            var data = new List<RuntimeFormula>[dfa.States.Length];
            for (int i = dfa.States.Length; i != 0;)
            {
                data[--i] = new List<RuntimeFormula>();
            }

            foreach (var state in dfa.States)
            {
                foreach (var item in state.Items)
                {
                    Check(state, item, data[state.Index]);
                }
            }

            this.data = Array.ConvertAll(data, fs => fs.ToArray());
        }

        private void Check(DotState state, DotItem item, List<RuntimeFormula> es)
        {
            if (item.IsReduce || item.ProductionId == grammar.AugmentedProduction.Index)
            {
                return;
            }

            var A = grammar.Symbols[item.NextToken];
            foreach (var inhProperty in grammar.InheritedProperties)
            {
                if (inhProperty.Symbol == A)
                {
                    var formula = GetDefiningFormula(inhProperty, item);
                    RuntimeFormula rf = ToRuntimeFormula(formula, inhProperty.Index, item);
                    es.Add(rf);
                }
            }
        }

        private RuntimeFormula ToRuntimeFormula(SemanticFormula formula, int inhIndex, DotItem item)
        {
            if (!formula.IsCopy || formula.IsCalledOnReduce)
            {
                throw new NotImplementedException("TODO");
            }

            var lhe = new InheritedRuntimeProperty(1, inhIndex);

            var arguments = new IRuntimeValue[formula.Arguments.Length];
            arguments[0] = ToRuntimeValue(
                            formula.Arguments[0],
                            grammar.Productions[item.ProductionId],
                            item.Position);

            return new RuntimeFormula(lhe, arguments, args => args[0]);
        }

        private IRuntimeValue ToRuntimeValue(
            ISemanticValue value,
            Production     prod,
            int            lhePosition)
        {
            var asReference = value as SemanticReference;
            if (asReference != null)
            {
                return ToRuntimeValue(asReference, prod, lhePosition);
            }

            var asConstant = value as SemanticConstant;
            if (asConstant != null)
            {
                return new RuntimeConstant(asConstant.Value);
            }

            throw new NotImplementedException($"{value.GetType().Name} is not supported on runtime.");
        }

        private IRuntimeValue ToRuntimeValue(
            SemanticReference reference,
            Production        production,
            int               shiftedPosition)
        {
            var rhePropertyName = reference.Name;
            Symbol rheSymbol = reference.ResolveSymbol();
            int rheOffset;
            if (reference.Position < 0)
            {
                rheOffset = shiftedPosition - reference.Position;
            }
            else
            {
                rheOffset = shiftedPosition - reference.Position;
            }

            var rheProperty = grammar.InheritedProperties.Find(rheSymbol, rhePropertyName);
            var result = new InheritedRuntimeProperty(rheOffset, rheProperty.Index);
            return result;
        }

        private SemanticFormula GetDefiningFormula(InheritedProperty inhProperty, DotItem item)
        {
            var production = grammar.Productions[item.ProductionId];
            var result = production.Semantics.SingleOrDefault(f => f.Lhe.Position == item.Position);
            if (result == null)
            {
                var msg = string.Format(
                    "Definition of {0} is missing in postion {1} of production {2}",
                    inhProperty,
                    item.Position,
                    production);

                throw new InvalidOperationException(msg);
            }
            return result;
        }

        public RuntimeFormula[][] GetData()
        {
            return this.data;
        }
    }
}
