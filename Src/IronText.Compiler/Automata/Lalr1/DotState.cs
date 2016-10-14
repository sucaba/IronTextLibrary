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

        private MutableDotItemSet cachedKernel;
        private int index;

        public DotState(int index, IEnumerable<DotItem> dotItems)
        {
            this.index = index;
            this.Items = new MutableDotItemSet(dotItems);
        }

        public int Index => index;

        public bool IsDeterministicReduce
        {
            get
            {
                return Transitions.Count == 0
                    && Items.Count == 1
                    && Items[0].IsReduce
                    && !Items[0].IsAugmented;
            }
        }

        public void Reindex(int newIndex)
        {
            this.index = newIndex;
        }

        public IDotItemSet KernelItems
        {
            get
            {
                if (cachedKernel == null)
                {
                    cachedKernel =
                        new MutableDotItemSet(
                            from item in Items where item.IsKernel select item);
                }

                return cachedKernel;
            }
        }

        public DotState GetNext(int token)
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

        public bool AddTransition(int token, DotState to)
        {
            if (Transitions.Any(t => t.Token == token))
            {
                return false;
            }

            Transitions.Add(new DotTransition(token, to));
            return true;
        }

        public DotItem GetItem(int prodId, int dotPos)
        {
            foreach (var item in Items)
            {
                if (item.ProductionId == prodId && item.Position == dotPos)
                {
                    return item;
                }
            }

            throw new InvalidOperationException("Internal error: dotitem not found");
        }
    }
}
