using System.Collections.Generic;
using System.Collections;
using IronText.Runtime;
using System;

namespace IronText.Automata.TurnPlanning
{
    class Plan
    {
        private readonly List<Turn> turns = new List<Turn>();

        public Plan(int outcome)
        {
            this.Outcome = outcome;
            this.IsAugmentedStart = outcome == PredefinedTokens.AugmentedStart;
        }

        public IEnumerable<Turn> KnownTurns => turns;

        public int Count => turns.Count;

        public bool IsAugmentedStart { get; }
        public int Outcome { get; }

        public void Add(Turn entry)
        {
            turns.Add(entry);
        }

        public Turn this[int index] => turns[index];

        public IEnumerable<int> GetTokensToConsume(int startPosition)
        {
            int count = turns.Count;
            for (int i = startPosition; i != count; ++i)
            {
                var token = turns[i].TokenToConsume;
                if (token.HasValue)
                {
                    yield return token.Value;
                }
            }
        }
    }
}
