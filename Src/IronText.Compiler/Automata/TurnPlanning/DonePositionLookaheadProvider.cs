using IronText.Common;
using IronText.Runtime;
using System.Linq;
using IronText.Algorithm;

namespace IronText.Automata.TurnPlanning
{
    class DonePositionLookaheadProvider
    {
        public TokenSetsRelation<TurnDfaState>     StateLookaheads { get; }

        public ITokenSetsRelation<TurnDfaSubstate> SubstateLookaheads =>
            substateLookaheads;

        private readonly TokenSetsRelation<TurnDfaSubstate> substateLookaheads;
        private readonly ImplMap<TurnDfaState, TurnDfaStateDetails> details;
        private readonly TurnDfaState[] dfaStates;

        public DonePositionLookaheadProvider(
            TurnDfa0Provider            dfa0,
            TransitionLookaheadProvider transitionLookaheads,
            SubcallLookaheadsAlgorithm  subcallLookaheads,
            TokenSetProvider            tokenSetProvider)
        {
            var tokenSet = tokenSetProvider.TokenSet;

            this.dfaStates = dfa0.States;
            this.details = dfa0.Details;
            this.substateLookaheads = new TokenSetsRelation<TurnDfaSubstate>(tokenSet);

            // TODO: Verify if Eoi lookahead is already present in spontaneous
            substateLookaheads.Add(
                details.Of(dfaStates[0]).KernelSubstates.First(),
                PredefinedTokens.Eoi);

            transitionLookaheads.Spontaneous.CopyTo(substateLookaheads);

            PropagateLookaheads(transitionLookaheads.Propagation);

            FillSubcallLookaheads(subcallLookaheads, tokenSet);

            StateLookaheads = new TokenSetsRelation<TurnDfaState>(tokenSet);

            FillReturnStatesLookaheads(tokenSetProvider.TokenSet);
        }

        private void FillReturnStatesLookaheads(BitSetType tokenSet)
        {
            var groups = substateLookaheads
                .Where(s => s.Key.PlanPosition.IsDone)
                .GroupBy(
					// Note: Logic is related to token DFA provider
                    s => s.Key.Owner,
                    pair => pair.Value)
                .ToArray();
            foreach (var g in groups)
            {
                StateLookaheads.Add(
                    g.Key,
                    g.Aggregate(
                        tokenSet.Empty,
                        (acc, x) => acc.Union(x)));
            }
        }

        private void FillSubcallLookaheads(SubcallLookaheadsAlgorithm algorithm, BitSetType tokenSet)
        {
            var temporaryLookaheads = new TokenSetsRelation<PlanPosition>(tokenSet);

            foreach (var state in dfaStates)
            {
                foreach (var substate in details.Of(state).Substates)
                {
                    temporaryLookaheads.Add(substate.PlanPosition, substateLookaheads.Of(substate));
                }

                algorithm.Fill(details.Of(state).Positions, temporaryLookaheads);
                foreach (var substate in details.Of(state).Substates)
                {
                    substateLookaheads.Add(substate, temporaryLookaheads.Of(substate.PlanPosition));
                }

                temporaryLookaheads.Clear();
            }
        }

        private void PropagateLookaheads(ManyToMany<TurnDfaSubstate> propagation)
        {
            bool modified;
            do
            {
                modified = false;
                foreach (var fromPoint in dfaStates.SelectMany(s => details.Of(s).KernelSubstates))
                {
                    var fromLA = substateLookaheads.Of(fromPoint);

                    foreach (var toPoint in propagation.Get(fromPoint))
                    {
                        var toLA = substateLookaheads.Of(toPoint);

                        if (!fromLA.IsSubsetOf(toLA))
                        {
                            substateLookaheads.Add(toPoint, fromLA);
                            modified = true;
                        }
                    }
                }
            }
            while (modified);
        }
    }
}
