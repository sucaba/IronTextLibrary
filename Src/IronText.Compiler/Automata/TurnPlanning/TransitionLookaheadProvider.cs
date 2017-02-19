using IronText.Common;
using IronText.Runtime;

namespace IronText.Automata.TurnPlanning
{
    class TransitionLookaheadProvider
    {
        public ManyToMany<TurnDfaSubstate> Propagation { get; }

        public TokenSetsRelation<TurnDfaSubstate> Spontaneous { get; }

        public TransitionLookaheadProvider(
            TurnDfa0Provider dfa,
            TurnNfa0Provider nfa,
            TokenSetProvider tokenSetProvider,
            SubcallLookaheadsAlgorithm subcallLookeahads)
        {
            var tokenSet = tokenSetProvider.TokenSet;

            this.Spontaneous = new TokenSetsRelation<TurnDfaSubstate>(tokenSet);
            this.Propagation = new ManyToMany<TurnDfaSubstate>();

            var temporaryLookaheads = new TokenSetsRelation<PlanPosition>(tokenSet);

            foreach (TurnDfaState fromState in dfa.States)
                foreach (TurnDfaSubstate fromKernel in dfa.Details.Of(fromState).KernelSubstates)
                {
                    temporaryLookaheads.Add(fromKernel.PlanPosition, PredefinedTokens.Propagated);

                    var fromPositions = nfa.WithSubcalls(fromKernel.PlanPosition);
                    subcallLookeahads.Fill(fromPositions, temporaryLookaheads);

                    foreach (PlanPosition fromPosition in fromPositions)
                    {
                        var fromSubstate = new TurnDfaSubstate(fromState, fromPosition);
                        var toSubstate = fromSubstate.Next();
                        if (toSubstate.PlanPosition.IsDone)
                        {
                            continue;
                        }

                        foreach (var lookahead in temporaryLookaheads.Of(fromPosition))
                        {
                            if (lookahead == PredefinedTokens.Propagated)
                            {
                                Propagation.Add(fromKernel, toSubstate);
                            }
                            else
                            {
                                Spontaneous.Add(toSubstate, lookahead);
                            }
                        }
                    }

                    temporaryLookaheads.Clear();
                }
        }
    }
}
