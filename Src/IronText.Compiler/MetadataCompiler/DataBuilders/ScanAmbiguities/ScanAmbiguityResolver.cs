using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using IronText.Automata.Regular;
using IronText.Framework;

namespace IronText.MetadataCompiler
{
    class ScanAmbiguityResolver : IScanAmbiguityResolver
    {
        private readonly TokenProducerInfo[] actionToTokenProducer;
        private readonly Dictionary<object, TokenProducerInfo> stateToTokenProducer;
        private readonly IntSetType tokenSetType;
        private MutableIntMap<int> ambTokenToMainToken;

        public ScanAmbiguityResolver(IntSetType tokenSetType, int actionCount)
        {
            this.actionToTokenProducer = new TokenProducerInfo[actionCount];
            this.stateToTokenProducer = new Dictionary<object, TokenProducerInfo>();
            this.tokenSetType = tokenSetType;
        }

        public void RegisterAction(int action, Disambiguation disamb, int mainToken, IEnumerable<int> tokens)
        {
            actionToTokenProducer[action] =
                new TokenProducerInfo
                {
                    MainTokenId     = mainToken,
                    Disambiguation  = disamb,
                    RealActions     = SparseIntSetType.Instance.Of(action),
                    PossibleTokens  = tokenSetType.Of(tokens)
                };
        }

        public void RegisterState(TdfaState state)
        {
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

        public void DefineAmbiguities(BnfGrammar grammar)
        {
            this.ambTokenToMainToken = new MutableIntMap<int>();

            foreach (var prod in stateToTokenProducer.Values.Distinct())
            {
                int ambTokenId = grammar.DefineAmbToken(prod.MainTokenId, prod.PossibleTokens);
                prod.State.EnvelopeId = ambTokenId;
                ambTokenToMainToken.Set(new IntArrow<int>(ambTokenId, prod.MainTokenId));
            }
        }
    }
}
