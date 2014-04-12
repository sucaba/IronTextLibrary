using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using IronText.Automata.Regular;
using IronText.Reflection;

namespace IronText.MetadataCompiler
{
    interface IScanAmbiguityResolver
    {
        /// <summary>
        /// Register tokens produced by an action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="disambiguation"></param>
        /// <param name="mainToken"></param>
        /// <param name="tokens"></param>
        void RegisterAction(Matcher matcher);

        /// <summary>
        /// Register actions invoked in state
        /// </summary>
        /// <param name="state"></param>
        /// 
        void RegisterState(TdfaState state);

        /// <summary>
        /// Define ambiguous symbols
        /// </summary>
        /// <param name="grammar"></param>
        void DefineAmbiguities(Grammar grammar);
    }

    class ScanAmbiguityResolver : IScanAmbiguityResolver
    {
        private readonly TokenProducerInfo[] actionToTokenProducer;
        private readonly Dictionary<object, TokenProducerInfo> stateToTokenProducer;
        private readonly IntSetType tokenSetType;

        public ScanAmbiguityResolver(IntSetType tokenSetType, int actionCount)
        {
            this.actionToTokenProducer = new TokenProducerInfo[actionCount];
            this.stateToTokenProducer = new Dictionary<object, TokenProducerInfo>();
            this.tokenSetType = tokenSetType;
        }

        public void RegisterAction(Matcher matcher)
        {
            SymbolBase      outcome = matcher.Outcome;
            AmbiguousSymbol ambiguous;
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
            else if ((ambiguous = outcome as AmbiguousSymbol) != null)
            {
                actionToTokenProducer[matcher.Index] =
                    new TokenProducerInfo
                    {
                        MainTokenId     = ambiguous.MainToken,
                        Disambiguation  = matcher.Disambiguation,
                        RealActions     = SparseIntSetType.Instance.Of(matcher.Index),
                        PossibleTokens  = tokenSetType.Of(ambiguous.Tokens)
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

        public void RegisterState(TdfaState state)
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

        public void DefineAmbiguities(Grammar grammar)
        {
            foreach (var prod in stateToTokenProducer.Values)
            {
                var ambSymbol = grammar.Symbols.FindOrAddAmbiguous(prod.MainTokenId, prod.PossibleTokens);
                prod.State.EnvelopeId = ambSymbol.Index;
#if DEBUG
                Debug.WriteLine("Created ambiguous symbol {0} for state #{1}", ambSymbol, prod.State.Index);
#endif
            }
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
