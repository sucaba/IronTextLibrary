using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework
{
    /// <summary>
    /// Ambiguous token
    /// </summary>
    public sealed class BnfAmbToken
    {
        public BnfAmbToken(int ambTokenId, int mainToken, IEnumerable<int> tokens)
        {
            this.Id = ambTokenId;
            this.MainToken = mainToken;
            this.Tokens = new ReadOnlyCollection<int>(tokens.ToArray());
        }

        /// <summary>
        /// ID of the ambiguous token
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The most probable mandatory token ID 
        /// or <c>-1</c> if there is no main token.
        /// </summary>
        public int MainToken { get; private set; }

        /// <summary>
        /// Token alternatives
        /// </summary>
        public ReadOnlyCollection<int> Tokens { get; private set; }
    }
}
