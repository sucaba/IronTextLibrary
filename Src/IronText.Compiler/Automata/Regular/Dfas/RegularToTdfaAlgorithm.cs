using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Extensibility;

namespace IronText.Automata.Regular
{
    using Action = System.Int32;
    using State = System.Int32;
    using System;

    public class RegularToTdfaAlgorithm
    {
        const State NoState = -1;

        private static readonly SparseIntSetType StateSetType = SparseIntSetType.Instance;
        private static readonly SparseIntSetType PositionSetType = SparseIntSetType.Instance;
        private readonly ITdfaData data;
        private IntSet ambiguous;
        private static readonly IntSet NewLines = UnicodeIntSetType.Instance.Of(
            UnicodeIntSetType.AsciiNewLine,
            UnicodeIntSetType.NextLineChar,
            UnicodeIntSetType.LineSeparatorChar,
            UnicodeIntSetType.ParagraphSeparatorChar);

        // 1. build full RegularTree (don't build first, following for positions)
        // 2. build regular alphabet (equivalense classes)
        // 3. build first, following for non-literal part of regular tree
        // 4. build non-literal TdfaData
        public RegularToTdfaAlgorithm(RegularTree noConstRegTree, Dictionary<string,Action> literalToAction)
        {
            var equivClasses = noConstRegTree
                                .GetEquivalenceCsets()
                                .Union(new [] { NewLines });
            var alphabet = new EquivalenceClassesAlphabet(equivClasses);

            foreach (var literal in literalToAction.Keys)
            {
                foreach (char ch in literal)
                {
                    alphabet.AddInputSet(SparseIntSetType.Instance.Of(ch));
                }
            }

            // Step 1. Convert the NFA for non-constant REs to a DFA using the usual 
            // algorithms for subset construction and state minimization [ASU86, WaG84].
            var initialDfa = new RegularToDfaAlgorithm(noConstRegTree, alphabet);
            this.data = initialDfa.Data;

#if false
            using (var view = new IronText.Diagnostics.GvGraphView(Guid.NewGuid() + ".gv"))
            {
                data.DescribeGraph(view);
            }
#endif

            // Step 2. Extend the DFA to a tunnel automaton by setting Tunnel (s) to NoState 
            // for every state s.
            int initialStateCount = data.StateCount;
            foreach (var S in data.EnumerateStates())
            {
                S.Tunnel = NoState;
            }
            
            // Step 3: Compute the set of ambiguous states of the tunnel automaton. 
            this.ambiguous = FindAmbiguousStates();

            // Step 4: For every constant RE execute Step 5 which incrementally extends 
            // the tunnel automaton. Continue with Step 6.
            foreach (var pair in literalToAction)
            {
                ExtendAutomatonWithLiteral(pair.Key, pair.Value);
            }

            var newlines = alphabet.Encode(NewLines);

            // Add new line handling
            foreach (var state in data.EnumerateStates())
            {
                var i = state.Outgoing.FindIndex(t => t.HasAnySymbol(newlines));
                if (i < 0)
                {
                    continue;
                }

                var newlineTransition = state.Outgoing[i];
                var to = data.GetState(newlineTransition.To);
                
                TdfaState newlineState;
                if (to.IsNewline)
                {
                    continue;
                }
                else if (data.EnumerateIncoming(to.Index).All(t => t.HasSingleSymbolFrom(newlines)))
                {
                    newlineState = to;
                }
                else
                {
                    newlineState = new TdfaState(data);
                    data.AddState(newlineState);
                    newlineState.Tunnel      = newlineTransition.To;
                    newlineState.IsAccepting = to.IsAccepting;
                    newlineState.Action      = to.Action;

                    data.DeleteTransition(state.Index, newlines);
                    data.AddTransition(state.Index, newlines, newlineState.Index);
                }

                newlineState.IsNewline = true;
            }
        }

        private static IEnumerable<int> EnumerateLiteralSymbols(IRegularAlphabet alphabet, string literal)
        {
            foreach (char ch in literal)
            {
                yield return alphabet.Encode(ch);
            }
        }

        private void ExtendAutomatonWithLiteral(string literal, int scanAction)
        {
            var state = data.Start;

            var symbols = EnumerateLiteralSymbols(data.Alphabet, literal).GetEnumerator();
            bool hasSymbol;

            // trace and do nothing
            while ((hasSymbol = symbols.MoveNext()))
            {
                State next = Control(state, symbols.Current);
                if (next == NoState || ambiguous.Contains(next))
                {
                    break;
                }

                state = next;
            }

            int previous = state;

            // trace and duplicate the path
            while (hasSymbol)
            {
                State next = Control(state, symbols.Current);
                if (next != NoState)
                {
                    state = next;

                    var newStateInfo = new TdfaState(data);
                    int newState = data.AddState(newStateInfo);
                    data.DeleteTransition(from: previous, symbol: symbols.Current);
                    data.AddTransition(
                            from: previous,
                            symbol: symbols.Current,
                            to: newState
                        );
                    var S = data.GetState(state);
                    newStateInfo.IsAccepting = S.IsAccepting;
                    newStateInfo.Action = S.Action;
                    newStateInfo.Tunnel = state;
                    previous = newState;

                    hasSymbol = symbols.MoveNext();
                }
                else
                {
                    var S = data.GetState(state);
                    if (S.Tunnel == NoState)
                    {
                        break;
                    }

                    state = S.Tunnel;
                }
            }

            // extend the path
            for (; hasSymbol; hasSymbol = symbols.MoveNext())
            {
                var newStateInfo = new TdfaState(data);
                int newState = data.AddState(newStateInfo);
                data.AddTransition(
                        from: previous,
                        symbol: symbols.Current,
                        to: newState
                    );
                newStateInfo.Tunnel = NoState;
                previous = newState;
            }

            // process new final state
            var finalState = data.GetState(previous);
            finalState.IsAccepting = true;
            finalState.Action = scanAction;
        }

        private State Control(State state, int symbol)
        {
            var S = data.GetState(state);
            foreach (var transition in S.Outgoing)
            {
                if (transition.Symbols.Contains(symbol))
                {
                    return transition.To;
                }
            }

            return NoState;
        }

        public ITdfaData Data { get { return data; } } 

        private IntSet FindAmbiguousStates()
        {
            var predCount = new int[data.StateCount];

            foreach (var S in data.EnumerateStates())
            {
                foreach (var transition in S.Outgoing)
                {
                    predCount[transition.To] += transition.Symbols.Count;
                }
            }

            var ambiguous = StateSetType.Range(0, data.StateCount - 1).EditCopy();
            var unabiguous = StateSetType.Of(data.Start).EditCopy();

            while (!unabiguous.IsEmpty)
            {
                var state = unabiguous.PopAny();
                ambiguous.Remove(state);

                var S = data.GetState(state);
                foreach (var transition in S.Outgoing)
                {
                    int successor = transition.To;
                    if (predCount[successor] == 1 && !unabiguous.Contains(successor))
                    {
                        unabiguous.Add(successor);
                    }
                }
            }

            return ambiguous.CompleteAndDestroy();
        }
    }
}
