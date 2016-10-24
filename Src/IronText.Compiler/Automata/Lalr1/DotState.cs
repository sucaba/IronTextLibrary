using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Compiler.Analysis;

namespace IronText.Automata.Lalr1
{
    public sealed class DotState
    {
        public static readonly DotState FailState = new DotState(-1, new DotItem[0]);

        public readonly IDotItemSet Items;
        public readonly List<DotTransition> Transitions = new List<DotTransition>();

        private IEnumerable<DotItem> cachedKernel;
        private int index;

        public DotState(int index, IEnumerable<DotItem> dotItems)
        {
            this.index = index;
            this.Items = new MutableDotItemSet(dotItems);
        }

        public int Index => index;

        public IEnumerable<DotItem> KernelItems =>
            cachedKernel
            ?? (cachedKernel =
                    Items.Where(item => item.IsKernel));

        public DotState Goto(int token)
        {
            foreach (var t in Transitions)
            {
                if (t.Token == token)
                {
                    return t.To;
                }
            }

            return FailState;
        }

        public bool AddGoto(int token, DotState to)
        {
            if (Transitions.Any(t => t.Token == token))
            {
                return false;
            }

            Transitions.Add(new DotTransition(token, to));
            return true;
        }

        public DotItem GetItem(DotItem item) => Items.First(item.Equals);
    }
}
