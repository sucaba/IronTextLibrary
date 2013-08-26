using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;

namespace IronText.Lib.RegularAst.Backend.InMemoryNfaCompiler
{
    enum NfaStateKind : int
    {
        CSet  = 0,
        Match = 1,
        Split = 2,
    }

    /// <summary>
    /// </summary>
    class NfaState
    {
        public NfaStateKind Kind;
        public IntSet CSet;
        public readonly List<NfaState> Out = new List<NfaState>();
        public int LastList;

        public NfaState(IntSet cset)
        {
            this.Kind = NfaStateKind.CSet;
            this.CSet = cset;
            this.Out = new List<NfaState>();
        }

        public NfaState(NfaStateKind kind)
        {
            this.Kind = kind;
            this.CSet = UnicodeIntSetType.Instance.Empty;
            this.Out = new List<NfaState>();
        }
    }

    class DfaState
    {
        public List<NfaState> List;
        public readonly DfaState[] Next = new DfaState[256];
    }

    /// <summary>
    /// A partially built NFA without the matching state filled in.
    /// </summary>
    internal class NfaFragment
    {
        public NfaState Start;
        public List<List<NfaState>> Outs;

        public NfaFragment()
        {
            Outs = new List<List<NfaState>>();
        }

        public static NfaFragment Cat(IEnumerable<NfaFragment> children)
        {
            var result = new NfaFragment();

            NfaFragment lastFragment = null;

            if (children.Count() == 0)
            {
                result.Start = new NfaState(NfaStateKind.Split);
                result.Outs.Add(result.Start.Out);
            }
            else
            {
                foreach (var fragment in children)
                {
                    if (lastFragment == null)
                    {
                        result.Start = fragment.Start;
                    }
                    else
                    {
                        foreach (var dangling in lastFragment.Outs)
                        {
                            dangling.Add(fragment.Start);
                        }
                    }

                    lastFragment = fragment;
                }

                if (lastFragment == null)
                {
                    result.Outs = new List<List<NfaState>>();
                }
                else
                {
                    result.Outs = new List<List<NfaState>>(lastFragment.Outs);
                }
            }

            return result;
        }

        public static NfaFragment Or(IEnumerable<NfaFragment> children)
        {
            var result = new NfaFragment();
            result.Start = new NfaState(NfaStateKind.Split);
            var output = new List<List<NfaState>>();
            foreach (var fragment in children)
            {
                result.Start.Out.Add(fragment.Start);
                result.Outs.Add(fragment.Start.Out);
            }

            return result;
        }

        public static NfaFragment ZeroOrOne(NfaFragment fragment)
        {
            NfaFragment result = new NfaFragment();
            result.Start = new NfaState(NfaStateKind.Split);
            result.Outs.Add(result.Start.Out);
            result.Start.Out.Add(fragment.Start);
            result.Outs.AddRange(fragment.Outs);
            return result;
        }

        public static NfaFragment ZeroOrMore(NfaFragment inner)
        {
            var s = new NfaState(NfaStateKind.Split);
            s.Out.Add(inner.Start);
            foreach (var danglingInnerOut in inner.Outs)
            {
                danglingInnerOut.Add(s);
            }

            var result = new NfaFragment();
            result.Start = s;
            result.Outs.Add(s.Out);
            return result;
        }
    }

    /// <summary>
    /// NFA with cached DFA states.
    /// </summary>
    internal class Nfa
    {
        private int listid;
        private List<NfaState> l1 = new List<NfaState>();
        private List<NfaState> l2 = new List<NfaState>();
        private List<DfaState> allDStates = new List<DfaState>();
        private readonly Dictionary<List<NfaState>, DfaState> cache;
        private readonly DfaState start;
        private readonly NfaState matchState = new NfaState(NfaStateKind.Match);

        public Nfa(AstNode program)
        {
            cache = new Dictionary<List<NfaState>, DfaState>(new ListCmp());

            var nfaFragment = new FragmentBuilder().Build(program);
            foreach (var end in nfaFragment.Outs)
            {
                end.Add(matchState);
            }

            this.start = GetDfaState(StartList(nfaFragment.Start, l1));
        }

        public bool Match(int[] input)
        {
            DfaState d, next;
            
            d = start;
            int len = input.Length;
            for (int i = 0; i != len; ++i)
            {
                var c = input[i];
                if ((next = d.Next[c]) == null)
                {
                    next = NextState(d, c);
                }

                d = next;
            }

            return IsMatch(d.List);
        }

        private DfaState NextState(DfaState d, int c)
        {
            Step(d.List, c, l1);
            DfaState result;
            lock (cache)
            {
                result = GetDfaState(l1);
                d.Next[c] = result;
            }

            return result;
        }

        private DfaState GetDfaState(List<NfaState> l)
        {
            DfaState result;
            lock (cache)
            {
                if (!cache.TryGetValue(l, out result))
                {
                    l = new List<NfaState>(l);
                    result = new DfaState { List = l };
                    cache[l] = result;
                }
            }

            return result;
        }

        public bool Match(NfaState start, IEnumerable<int> input)
        {
            List<NfaState> clist, nlist, t;
            clist = StartList(start, l1);
            nlist = l2;
            foreach (int ch in input)
            {
                Step(clist, ch, nlist);
                t = clist;
                clist = nlist;
                nlist = t;
            }

            return IsMatch(clist);
        }

        private List<NfaState> StartList(NfaState s, List<NfaState> l)
        {
            ++listid;
            l.Clear();
            AddState(l, s);
            return l;
        }

        private bool IsMatch(List<NfaState> clist)
        {
            foreach (NfaState s in clist)
            {
                if (s.Kind == NfaStateKind.Match)
                {
                    return true;
                }
            }

            return false;
        }

        public void Step(List<NfaState> clist, int ch, List<NfaState> nlist)
        {
            ++listid;
            nlist.Clear();

            foreach (var s in clist)
            {
                if (s.CSet.Contains(ch))
                {
                    AddStates(nlist, s.Out);
                }
            }
        }

        private void AddStates(List<NfaState> l, IEnumerable<NfaState> states)
        {
            foreach (var s in states)
            {
                AddState(l, s);
            }
        }

        private void AddState(List<NfaState> l, NfaState s)
        {
            if (s == null || s.LastList == listid)
            {
                return;
            }

            s.LastList = listid;

            if (s.Kind == NfaStateKind.Split)
            {
                AddStates(l, s.Out);
            }
            else
            {
                l.Add(s);
            }
        }

        class ListCmp : IEqualityComparer<List<NfaState>>
        {
            public bool Equals(List<NfaState> x, List<NfaState> y)
            {
                int count = x.Count;

                if (count != y.Count)
                {
                    return false;
                }

                for (int i = 0; i != count; ++i)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(List<NfaState> obj)
            {
                unchecked { return obj.Sum(x => x.GetHashCode()); }
            }
        }
    }
}
