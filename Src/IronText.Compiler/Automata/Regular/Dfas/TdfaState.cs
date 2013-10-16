using System.Collections.Generic;
using IronText.Algorithm;
using System.Collections.ObjectModel;
using System.Linq;
using IronText.Extensibility;

namespace IronText.Automata.Regular
{
    public sealed class TdfaState : IScannerState
    {
        private ReadOnlyCollection<IScannerTransition> transitions;
        private readonly ITdfaData container;
        private List<int> _actions;

        public TdfaState(ITdfaData container)
        {
            this.container = container;
        }

        public int Index { get; set; }

        public IntSet Positions { get; set; }

        public bool IsAccepting { get; set; }

        public bool IsNewline { get; set; }

        public List<int> Actions
        {
            get { return _actions ?? (_actions = new List<int>()); }
        }

        public int? Action
        {
            get
            {
                if (_actions == null || _actions.Count == 0)
                {
                    return null;
                }

                return _actions[0];
            }
        }

        public int Tunnel { get; set; }

        /// <summary>
        /// Determinies whether it is state without outgoing transitions and no tunnel transition.
        /// Such state should be also accepting.
        /// </summary>
        public bool IsFinal
        {
            get { return Outgoing.Count == 0 && Tunnel < 0; }
        }

        public readonly List<TdfaTransition> Outgoing = new List<TdfaTransition>();

        IScannerState IScannerState.TunnelState
        {
            get { return container.GetState(Tunnel); }
        }

        ReadOnlyCollection<IScannerTransition> IScannerState.Transitions
        {
            get
            {
                if (transitions == null)
                {
                    transitions = new ReadOnlyCollection<IScannerTransition>(
                        Outgoing.Select(MakeScannerTransition).ToArray());
                }

                return transitions;
            }
        }

        private IScannerTransition MakeScannerTransition(TdfaTransition transition)
        {
            var charSet = container.Alphabet.Decode(transition.Symbols);
            return new ScannerTransition(
                charSet.EnumerateIntervals().Select(MakeCharRange), 
                container.GetState(transition.To));
        }

        private static CharRange MakeCharRange(IntInterval interval)
        {
            checked
            {
                return new CharRange((char)interval.First, (char)interval.Last);
            }
        }
    }
}
