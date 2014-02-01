using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Compiler;
using IronText.Compiler.Analysis;
using IronText.Framework;
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
        void RegisterAction(Matcher scanProduction);

        /// <summary>
        /// Register actions invoked in state
        /// </summary>
        /// <param name="state"></param>
        /// 
        void RegisterState(TdfaState state);

        /// <summary>
        /// Define ambiguous tokens
        /// </summary>
        /// <param name="grammar"></param>
        void DefineAmbiguities(EbnfGrammar grammar);
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

        public void RegisterAction(Matcher scanProduction)
        {
            SymbolBase      outcome = scanProduction.Outcome;
            AmbiguousSymbol ambiguous;
            Symbol          deterministic;

            if (outcome == null)
            {
                actionToTokenProducer[scanProduction.Index] =
                    new TokenProducerInfo
                    {
                        MainTokenId     = -1,
                        Disambiguation  = scanProduction.Disambiguation,
                        RealActions     = SparseIntSetType.Instance.Of(scanProduction.Index),
                        PossibleTokens  = tokenSetType.Empty
                    };
            
            }
            else if ((ambiguous = outcome as AmbiguousSymbol) != null)
            {
                actionToTokenProducer[scanProduction.Index] =
                    new TokenProducerInfo
                    {
                        MainTokenId     = ambiguous.MainToken,
                        Disambiguation  = scanProduction.Disambiguation,
                        RealActions     = SparseIntSetType.Instance.Of(scanProduction.Index),
                        PossibleTokens  = tokenSetType.Of(ambiguous.Tokens)
                    };
            }
            else if ((deterministic = outcome as Symbol) != null)
            {
                actionToTokenProducer[scanProduction.Index] =
                    new TokenProducerInfo
                    {
                        MainTokenId     = deterministic.Index,
                        Disambiguation  = scanProduction.Disambiguation,
                        RealActions     = SparseIntSetType.Instance.Of(scanProduction.Index),
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

        public void DefineAmbiguities(EbnfGrammar grammar)
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
