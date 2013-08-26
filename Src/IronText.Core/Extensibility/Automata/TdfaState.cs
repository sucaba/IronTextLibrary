using System.Collections.Generic;
using IronText.Algorithm;

namespace IronText.Extensibility
{
    public class TdfaState
    {
        public int    Index;
        public IntSet Positions;
        public bool   IsAccepting;
        public bool   IsNewline;
        public int?   Action;
        public int    Tunnel;

        /// <summary>
        /// Determinies whether it is state without outgoing transitions and no tunnel transition.
        /// Such state should be also accepting.
        /// </summary>
        public bool   IsFinal
        {
            get { return Outgoing.Count == 0 && Tunnel < 0; }
        }

        public readonly List<TdfaTransition> Outgoing = new List<TdfaTransition>();
    }
}
