using IronText.Automata.Lalr1;
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
                    RuntimeFormula rf = ToRuntimeFormulaWithStackInput(formula, inhProperty.Index, item);
                    es.Add(rf);
                }
            }
        }

        private RuntimeFormula ToRuntimeFormulaWithStackInput(SemanticFormula formula, int inhIndex, DotItem item)
        {
            if (!formula.IsCopy || formula.IsCalledOnReduce)
            {
                throw new NotImplementedException("TODO");
            }

            var lhe = new InheritedRuntimeProperty(1, inhIndex);
            int lheProductionPosition = item.Position;

            var arguments = new IRuntimeValue[formula.ActualRefs.Length];
            arguments[0] = GetRuntimeReference(
                            formula.ActualRefs[0],
                            item,
                            lheProductionPosition);

            return new RuntimeFormula(lhe, arguments, args => args[0]);
        }

        private InheritedRuntimeProperty GetRuntimeReference(
            SemanticReference rheArg,
            DotItem item,
            int lheProductionPosition)
        {
            var rhePropertyName = rheArg.Name;
            var prod = grammar.Productions[item.ProductionId];
            Symbol rheSymbol;
            int rheOffset;
            if (rheArg.Position < 0)
            {
                rheOffset = lheProductionPosition - rheArg.Position;
                rheSymbol = prod.Outcome;
            }
            else
            {
                rheSymbol = prod.Input[rheArg.Position];
                rheOffset = lheProductionPosition - rheArg.Position;
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
