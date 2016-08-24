using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Runtime;
using IronText.Collections;

namespace IronText.Automata.Lalr1
{
    class CanonicalLrDfaTable : ILrParserTable
    {
        private readonly GrammarAnalysis               grammar;
        private readonly ParserConflictResolver        conflictResolver;
        private readonly IMutableTable<ParserDecision> decisionTable;

        public CanonicalLrDfaTable(
            ILrDfa          dfa,
            GrammarAnalysis grammar,
            ParserConflictResolver conflictResolver)
        {
            this.grammar = grammar;
            this.conflictResolver = conflictResolver;
            this.decisionTable = new MutableTable<ParserDecision>(
                                    dfa.States.Length,
                                    grammar.TotalSymbolCount);

            FillDfaTable(dfa.States);
            HasUnresolvedTerminalAmbiguities = FillAmbiguousTerminalActions(dfa.States);
        }

        public bool HasUnresolvedTerminalAmbiguities { get; }

        public ITable<ParserDecision> DecisionTable => decisionTable;

        private void FillDfaTable(DotState[] states)
        {
            foreach (var state in states)
            {
                foreach (var item in state.Items)
                {
                    if (!item.IsReduce)
                    {
                        foreach (var transition in item.Transitions)
                        {
                            AssignAction(
                                state.Index,
                                transition.Token,
                                ParserInstruction.Shift(
                                    state.GetNextIndex(transition.Token)));
                        }
                    }
                    else if (item.IsAugmented)
                    {
                        AssignAction(
                            state.Index,
                            PredefinedTokens.Eoi,
                            ParserInstruction.AcceptAction);
                    }
                    else
                    {
                        var action = ParserInstruction.Reduce(item.ProductionId);

                        foreach (var lookahead in item.LA)
                        {
                            AssignAction(
                                state.Index,
                                lookahead,
                                action);
                        }
                    }
                }
            }
        }

        private void AssignAction(int state, int token, ParserInstruction action)
        {
            AssignAction(state, token, new ParserDecision(action));
        }

        private void AssignAction(int state, int token, ParserDecision decision)
        {
            ParserDecision current = decisionTable.Get(state, token);
            ParserDecision resolved;

            if (current == ParserDecision.NoAlternatives)
            {
                resolved = decision;
            }
            else if (current.Equals(decision))
            {
                resolved = current;
            }
            else if (
                current.Instructions.Count == 1
                && conflictResolver.TryResolve(current, decision, token, out resolved))
            {
            }
            else
            {
                resolved = current.Alternate(decision);
            }

            decisionTable.Set(state, token, resolved);
        }

        private bool FillAmbiguousTerminalActions(DotState[] states)
        {
            bool result = false;

            for (int i = 0; i != states.Length; ++i)
            {
                var state = states[i];

                foreach (var ambiguousTerm in grammar.AmbiguousSymbols)
                {
                    var tokenToDecision = new Dictionary<int,ParserDecision>();
                    foreach (int token in ambiguousTerm.Alternatives)
                    {
                        var decision = decisionTable.Get(i, token);
                        if (decision != ParserDecision.NoAlternatives)
                        {
                            tokenToDecision.Add(token, decision);
                        }
                    }

                    switch (tokenToDecision.Values.Distinct().Count())
                    {
                        case 0:
                            // AmbToken is entirely non-acceptable for this state
                            decisionTable.Set(
                                i,
                                ambiguousTerm.EnvelopeIndex,
                                ParserDecision.NoAlternatives);
                            break;
                        case 1:
                            {
                                var pair = tokenToDecision.First();
                                if (pair.Key == ambiguousTerm.MainToken)
                                {
                                    // ambiguousToken action is the same as for the main token
                                    decisionTable.Set(
                                        i,
                                        ambiguousTerm.EnvelopeIndex,
                                        pair.Value);
                                }
                                else
                                {
                                    // Resolve ambToken to a one of the underlying tokens.
                                    // In runtime transition will be acceptable when this token
                                    // is in Msg and non-acceptable when this particular token
                                    // is not in Msg.
                                    var action = ParserInstruction.Resolve(pair.Key);
                                    decisionTable.Set(
                                        i,
                                        ambiguousTerm.EnvelopeIndex,
                                        new ParserDecision(action));
                                }
                            }

                            break;
                        default:
                            // GLR parser is required to handle terminal token alternatives.
                            result = true;

                            // No needed for GLR but but for the sake of explicitness
                            decisionTable.Set(
                                i,
                                ambiguousTerm.EnvelopeIndex,
                                ParserDecision.NoAlternatives);
                            break;
                    }
                }
            }

            return result;
        }
    }
}
