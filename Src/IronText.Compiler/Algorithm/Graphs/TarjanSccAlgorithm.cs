using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
    class TarjanSccAlgorithm<TVertex> where TVertex : IEquatable<TVertex>
    {
        private readonly List<List<TVertex>> sccs;
        private readonly Stack<TVertex> S;
        private readonly Dictionary<TVertex,VertexVisit> visited;
        private readonly Func<TVertex,IEnumerable<TVertex>> following;
        private int index;

        public TarjanSccAlgorithm(IEnumerable<TVertex> starts, Func<TVertex,IEnumerable<TVertex>> following)
        {
            this.following = following;
            this.sccs = new List<List<TVertex>>();
            this.S = new Stack<TVertex>();

            index = 0;
            var allVertexes = Graph.AllVertexes(starts, following);
            foreach (TVertex v in allVertexes)
            {
                if (Visited(v).index < 0)
                {
                    StrongConnect(v);
                }
            }

            Result = sccs;
        }

        public List<List<TVertex>> Result { get; private set; }

        private VertexVisit Visited(TVertex v)
        {
            VertexVisit result;
            if (!visited.TryGetValue(v, out result))
            {
                result = new VertexVisit { lowlink = -1, index = -1 };
            }

            return result;
        }

        private void StrongConnect(TVertex v)
        {
            Visited(v).index = index;
            Visited(v).lowlink = index;
            index++;
            S.Push(v);

            foreach (TVertex w in following(v))
            {
                if (Visited(w).index < 0)
                {
                    StrongConnect(w);
                    Visited(v).lowlink = Math.Min(Visited(v).lowlink, Visited(w).lowlink);
                }
                else if (S.Contains(w))
                {
                    Visited(v).lowlink = Math.Min(Visited(v).lowlink, Visited(w).index);
                }
            }

            if (Visited(v).lowlink == Visited(v).index)
            {
                List<TVertex> scc = new List<TVertex>();
                TVertex w;
                do
                {
                    w = S.Pop();
                    scc.Add(w);
                } 
                while (v.Equals(w));
                sccs.Add(scc);
            }
        }

        class VertexVisit
        {
            public int index;
            public int lowlink;
        }
    }
}
