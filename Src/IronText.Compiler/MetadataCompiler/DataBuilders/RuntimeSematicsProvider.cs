using IronText.Automata.Lalr1;
using IronText.Reflection;
using IronText.Runtime.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Automata.DotNfa;

namespace IronText.MetadataCompiler
{
    internal class RuntimeSemanticsProvider
    {
        private readonly Grammar grammar;

        public RuntimeSemanticsProvider(Grammar grammar, ILrDfa dfa)
        {
            this.grammar = grammar;

            this.StateToFormulas      = BuildShiftFormulas(dfa);
            this.ProductionToFormulas = BuildProductionFormulas();
        }

        public RuntimeFormula[][] StateToFormulas { get; }

        public RuntimeFormula[][] ProductionToFormulas { get; }

        private RuntimeFormula[][] BuildProductionFormulas()
        {
            var result = new List<RuntimeFormula>[grammar.Productions.AllCount];
            for (int i = result.Length; i != 0;)
            {
                result[--i] = new List<RuntimeFormula>();
            }

            foreach (var production in grammar.Productions)
            {
                var formulas = production.Semantics
                                    .Where(f => f.IsCalledOnReduce)
                                    .Select(f => ToRuntimeFormula(f, production));

                result[production.Index].AddRange(formulas);
            }

            return Array.ConvertAll(result, fs => fs.ToArray());
        }

        private RuntimeFormula[][] BuildShiftFormulas(ILrDfa dfa)
        {
            var result = new List<RuntimeFormula>[dfa.States.Length];
            for (int i = result.Length; i != 0;)
            {
                result[--i] = new List<RuntimeFormula>();
            }

            foreach (var state in dfa.States)
            {
                foreach (var item in state.Items)
                {
                    Check(state, item, result[state.Index]);
                }
            }

            // What about inlined states (inl-prod-index, pos-in-prod) ?

            return Array.ConvertAll(result, fs => fs.ToArray());
        }

        private void Check(DotState state, DotItem item, List<RuntimeFormula> es)
        {
            if (item.IsAugmented)
            {
                return;
            }

            foreach (var transition in item.GotoTransitions)
            {
                var A = grammar.Symbols[transition.Token];
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
        }

        private RuntimeFormula ToRuntimeFormula(SemanticFormula formula, Production prod)
        {
            if (!formula.IsCopy)
            {
                throw new NotImplementedException("TODO");
            }

            var lhe = formula.Lhe.ToRuntime(prod.InputLength);
            var rhe = formula.Arguments[0].ToRuntime(prod.InputLength);
            var result = new RuntimeFormula(lhe, new[] { rhe }, args => args[0]);
            return result;
        }

        private RuntimeFormula ToRuntimeFormula(SemanticFormula formula, int inhIndex, DotItem item)
        {
            if (!formula.IsCopy)
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
            int            currentPosition)
        {
            var asReference = value as SemanticReference;
            if (asReference != null)
            {
                return asReference.ToRuntime(currentPosition);
            }

            var asConstant = value as SemanticConstant;
            if (asConstant != null)
            {
                return new RuntimeConstant(asConstant.Value);
            }

            throw new NotImplementedException($"{value.GetType().Name} is not supported on runtime.");
        }

        private SemanticFormula GetDefiningFormula(InheritedProperty inhProperty, DotItem item)
        {
            var production = grammar.Productions[item.ProductionId];
            var result = production.Semantics
                            .SingleOrDefault(f => f.Lhe.Position == item.Position
                                               && f.Lhe.Name == inhProperty.Name);
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
    }
}
