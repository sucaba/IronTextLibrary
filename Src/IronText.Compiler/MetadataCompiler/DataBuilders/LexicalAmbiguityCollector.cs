using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using IronText.Automata.Regular;
using IronText.Reflection;
using IronText.Compiler.Analysis;

namespace IronText.MetadataCompiler
{
    interface ILexicalAmbiguityCollector
    {
        IEnumerable<AmbTokenInfo> CollectAmbiguities();
    }

    class LexicalAmbiguityCollector : ILexicalAmbiguityCollector
    {
        private readonly TokenProducerInfo[] actionToTokenProducer;
        private readonly Dictionary<object, TokenProducerInfo> stateToTokenProducer;
        private readonly IntSetType tokenSetType;
        private readonly Grammar grammar;
        private readonly ITdfaData tdfa;

        public LexicalAmbiguityCollector(Grammar grammar, ITdfaData tdfa)
        {
            this.grammar               = grammar;
            this.tdfa                  = tdfa;
            this.actionToTokenProducer = new TokenProducerInfo[grammar.Matchers.IndexCount];
            this.stateToTokenProducer  = new Dictionary<object, TokenProducerInfo>();
            this.tokenSetType          = new BitSetType(grammar.Symbols.IndexCount);
        }

        public IEnumerable<AmbTokenInfo> CollectAmbiguities()
        {
            // For each action store information about produced tokens
            foreach (var matcher in grammar.Matchers)
            {
                RegisterAction(matcher);
            }

            // For each 'ambiguous scanner state' deduce all tokens
            // which can be produced in this state.
            foreach (var state in tdfa.EnumerateStates())
            {
                RegisterState(state);
            }

            return DefineAmbiguities();
        }

        /// <summary>
        /// Register tokens produced by an action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="disambiguation"></param>
        /// <param name="mainToken"></param>
        /// <param name="tokens"></param>
        private void RegisterAction(Matcher matcher)
        {
            var outcome = matcher.Outcome;
            AmbiguousTerminal ambiguous;
            Symbol          deterministic;

            if (outcome == null)
            {
                actionToTokenProducer[matcher.Index] =
                    new TokenProducerInfo
                    {
                        MainTokenId     = -1,
                        Disambiguation  = matcher.Disambiguation,
                        RealActions     = SparseIntSetType.Instance.Of(matcher.Index),
                        PossibleTokens  = tokenSetType.Empty
                    };
            }
            else if ((ambiguous = outcome as AmbiguousTerminal) != null)
            {
                actionToTokenProducer[matcher.Index] =
                    new TokenProducerInfo
                    {
                        MainTokenId     = ambiguous.Main.Index,
                        Disambiguation  = matcher.Disambiguation,
                        RealActions     = SparseIntSetType.Instance.Of(matcher.Index),
                        PossibleTokens  = tokenSetType.Of(ambiguous.Alternatives.Select(alt => alt.Index))
                    };
            }
            else if ((deterministic = outcome as Symbol) != null)
            {
                actionToTokenProducer[matcher.Index] =
                    new TokenProducerInfo
                    {
                        MainTokenId     = deterministic.Index,
                        Disambiguation  = matcher.Disambiguation,
                        RealActions     = SparseIntSetType.Instance.Of(matcher.Index),
                        PossibleTokens  = tokenSetType.Of(deterministic.Index)
                    };
            }
        }

        /// <summary>
        /// Register actions invoked in state
        /// </summary>
        /// <param name="state"></param>
        private void RegisterState(TdfaState state)
        {
            if (!state.IsAccepting)
            {
                return;
            }

            var orderedActions = state.Actions;

            var stateTokenProducers =
                from act in orderedActions
                select actionToTokenProducer[act];

            var stateTokenProducer = TokenProducerInfo.Combine(tokenSetType, stateTokenProducers);
            switch (stateTokenProducer.PossibleTokens.Count)
            {
                case 0:
                    state.EnvelopeId = -1;
                    break;
                case 1:
                    state.EnvelopeId = stateTokenProducer.PossibleTokens.First();
                    break;
                default:
                    stateTokenProducer.State = state;
                    stateToTokenProducer[state] = stateTokenProducer;
                    break;
            }

            state.Actions.Clear();
            state.Actions.AddRange(stateTokenProducer.RealActions);
            state.Actions.Sort();
        }

        /// <summary>
        /// Define ambiguous symbols
        /// </summary>
        private IEnumerable<AmbTokenInfo> DefineAmbiguities()
        {
            var result = new List<AmbTokenInfo>();
            int index = grammar.Symbols.IndexCount;

            foreach (var prod in stateToTokenProducer.Values)
            {
                var amb = new AmbTokenInfo(index, prod.MainTokenId, prod.PossibleTokens);
                prod.State.EnvelopeId = index;
                result.Add(amb);
                ++index;

#if DEBUG
                Debug.WriteLine("Created ambiguous symbol #{0} for state #{1}", amb.Index, prod.State.Index);
#endif
            }

            return result;
        }

        class TokenProducerInfo
        {
            public TokenProducerInfo()
            {
                MainTokenId = PredefinedTokens.NoToken;
            }

            public TdfaState State { get; set; }

            public Disambiguation Disambiguation { get; set; }

            /// <summary>
            /// The most probable token id or <c>-1</c> if there is no token to distinguish.
            /// </summary>
            public int MainTokenId { get; set; }

            /// <summary>
            /// All tokens which can be produced by this rule.
            /// Can be empty for void-rules.
            /// </summary>
            public IntSet PossibleTokens { get; set; }

            public IntSet RealActions { get; set; }

            public override int GetHashCode()
            {
                return unchecked(MainTokenId + PossibleTokens.Count + RealActions.Count);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as TokenProducerInfo);
            }

            public bool Equals(TokenProducerInfo other)
            {
                return other != null
                    && MainTokenId == other.MainTokenId
                    && PossibleTokens.SetEquals(other.PossibleTokens)
                    && RealActions.SetEquals(other.RealActions);
            }

            public static TokenProducerInfo Combine(IntSetType tokenSetType, IEnumerable<TokenProducerInfo> items)
            {
                switch (items.Count())
                {
                    case 0: return
                            new TokenProducerInfo
                            {
                                Disambiguation = Disambiguation.Exclusive,
                                RealActions = SparseIntSetType.Instance.Empty,
                                PossibleTokens = tokenSetType.Empty,
                                MainTokenId = -1,
                            };
                    case 1: return items.First();
                }

                TokenProducerInfo result = null;

                foreach (var item in items)
                {
                    if (item.Disambiguation == Disambiguation.Exclusive)
                    {
                        if (result == null)
                        {
                            result = item;
                        }
                    }
                }

                if (result == null)
                {
                    result = new TokenProducerInfo { Disambiguation = Disambiguation.Exclusive };
                    int mainToken = PredefinedTokens.NoToken;
                    var allPossible = tokenSetType.Mutable();
                    var allActions = SparseIntSetType.Instance.Mutable();

                    foreach (var item in items)
                    {
                        if (mainToken != PredefinedTokens.NoToken && item.MainTokenId != PredefinedTokens.NoToken)
                        {
                            mainToken = item.MainTokenId;
                        }

                        allPossible.AddAll(item.PossibleTokens);
                        allActions.AddAll(item.RealActions);
                    }

                    result.MainTokenId = mainToken;
                    result.RealActions = allActions.CompleteAndDestroy();
                    result.PossibleTokens = allPossible.CompleteAndDestroy();
                }

                return result;
            }
        }
    }
}
