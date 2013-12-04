using System.Collections.Generic;
using System.Linq;
using IronText.Compiler.Analysis;

namespace IronText.Automata.Lalr1
{
    public sealed class MutableDotItemSet 
        : IDotItemSet
        , IMutableDotItemSet
    {
        private readonly List<DotItem> items;

        public MutableDotItemSet() { items = new List<DotItem>(); }

        public MutableDotItemSet(IEnumerable<DotItem> items) 
        { 
            this.items = new List<DotItem>(items);
        }

        public void Add(DotItem item)
        {
            items.Add(item);
        }

        public override bool Equals(object other)
        {
            return Equals(other as IDotItemSet);
        }

        public bool Equals(IDotItemSet other)
        {
            if (other == null || Count != other.Count)
            {
                return false;
            }

            for (int i = 0; i != Count; ++i)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var first = this.FirstOrDefault();
                return Count + (first == null ? 0 : first.GetHashCode());
            }
        }

        public override string ToString()
        {
            return string.Join(" ", this.Select(it => it.ToString()).ToArray());
        }

        public int Count { get { return items.Count; } }

        public DotItem this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        public IEnumerator<DotItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public void AddRange(IEnumerable<DotItem> items)
        {
            this.items.AddRange(items);
        }

        public int IndexOf(DotItem item)
        {
            return items.IndexOf(item);
        }
    }
}
