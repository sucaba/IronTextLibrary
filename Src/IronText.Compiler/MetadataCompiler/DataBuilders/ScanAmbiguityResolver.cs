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
    }
}
