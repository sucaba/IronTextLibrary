using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using IronText.Framework;

namespace IronText.MetadataCompiler
{
    public class TokenProducerInfo
    {
        public TokenProducerInfo()
        {
            MainTokenId    = BnfGrammar.NoToken;
        }

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
                int mainToken = BnfGrammar.NoToken;
                var allPossible = tokenSetType.Mutable();
                var allActions = SparseIntSetType.Instance.Mutable();

                foreach (var item in items)
                {
                    if (mainToken != BnfGrammar.NoToken && item.MainTokenId != BnfGrammar.NoToken)
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
