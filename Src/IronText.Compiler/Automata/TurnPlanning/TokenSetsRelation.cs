using IronText.Algorithm;
using System;
using System.Collections.Generic;
using System.Collections;
using IronText.Common;

namespace IronText.Automata.TurnPlanning
{
    class TokenSetsRelation<T> : IEnumerable<KeyValuePair<T,MutableIntSet>>
    {
        private Dictionary<T, MutableIntSet> positionToLA
            = new Dictionary<T, MutableIntSet>();

        private readonly BitSetType tokenSet;

        public TokenSetsRelation(BitSetType tokenSet)
        {
            this.tokenSet = tokenSet;
        }

        public TokenSetsRelation(TokenSetProvider tokenSetProvider)
            : this(tokenSetProvider.TokenSet)
        {
        }

        public void Clear()
        {
            positionToLA.Clear();
        }

        public void CopyTo(TokenSetsRelation<T> lookaheads)
        {
            foreach (var entry in positionToLA)
            {
                lookaheads.Add(entry.Key, entry.Value);
            }
        }

        public IntSet Of(T position)
        {
            MutableIntSet la;
            if (positionToLA.TryGetValue(position, out la))
            {
                return la;
            }

            return tokenSet.Empty;
        }

        public bool Add(T position, int token)
        {
            return GetMutable(position).Add(token);
        }

        public int Add(T position, IntSet tokens)
        {
            return GetMutable(position).AddAll(tokens);
        }

        public int Add(T position, IEnumerable<int> tokens)
        {
            int result = 0;
            var la = GetMutable(position);

            foreach (var token in tokens)
            {
                if (la.Add(token))
                {
                    ++result;
                }
            }

            return result;
        }

        public MutableIntSet GetMutable(T position) =>
            positionToLA.GetOrAdd(position, tokenSet.Mutable);

        IEnumerator<KeyValuePair<T, MutableIntSet>> IEnumerable<KeyValuePair<T, MutableIntSet>>.GetEnumerator()
        {
            return positionToLA.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return positionToLA.GetEnumerator();
        }
    }
}
