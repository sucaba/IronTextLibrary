using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Algorithm
{
    static class Graph
    {
#if false
        public static List<List<T>> Scc<T>(IEnumerable<T> startItems, Func<T, IEnumerable<T>> following)
            where T : IEquatable<T>
        {
            var tarjan = new TarjanSccAlgorithm<T>(startItems, following);
            return tarjan.Result;
        }
#endif

        public static IEnumerable<T> ToplogicalSort<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> following)
        {
            var result = new List<T>();
            var visited = new HashSet<T>();

            foreach (var item in source)
            {
                TopoVisit(item, visited, result, following);
            }

            return result;
        }

        private static void TopoVisit<T>(
            T           item,
            HashSet<T>  visited,
            List<T>     sorted,
            Func<T,IEnumerable<T>> following)
        {
            if (visited.Contains(item))
            {
                return;
            }

            visited.Add( item );

            foreach (var dep in following(item))
            {
                TopoVisit(dep, visited, sorted, following);
            }

            sorted.Add( item );
        }

        public static T[] AllVertexes<T>(T start, Func<T, IEnumerable<T>> following)
        {
            return AllVertexes(start, following, EqualityComparer<T>.Default);
        }

        public static T[] AllVertexes<T>(T start, Func<T, IEnumerable<T>> following, IEqualityComparer<T> cmp)
        {
            return AllVertexes(new[] { start }, following, cmp);
        }

        public static T[] AllVertexes<T>(IEnumerable<T> startItems, Func<T, IEnumerable<T>> following)
        {
            return AllVertexes(startItems, following, EqualityComparer<T>.Default);
        }

        public static T[] AllVertexes<T>(IEnumerable<T> startItems, Func<T, IEnumerable<T>> following, IEqualityComparer<T> cmp)
        {
            var result = new List<T>();
            AddAllVertexes(startItems, following, result, cmp);
            return result.ToArray();
        }

        public static void AddAllVertexes<T>(IEnumerable<T> startItems, Func<T, IEnumerable<T>> following, List<T> output, IEqualityComparer<T> cmp)
        {
            output.AddRange(startItems);
            AddAllVertexes(following, output, 0, cmp);
        }

        public static void AddAllVertexes<T>(Func<T, IEnumerable<T>> following, List<T> output, int frontIndex)
        {
            AddAllVertexes(following, output, frontIndex, EqualityComparer<T>.Default);
        }

        public static void AddAllVertexes<T>(Func<T, IEnumerable<T>> following, List<T> output, int frontIndex, IEqualityComparer<T> cmp)
        {
            while (frontIndex != output.Count)
            {
                var item = output[frontIndex++];
                var f = following(item).ToArray();
                f = f.Except(output, cmp).ToArray();
                output.AddRange(f);
            }
        }

        public static T[] Search<T>(T start, Func<T, IEnumerable<T>> following, Predicate<T> match)
        {
            return Search(new [] { start }, following, match);
        }

        public static T[] Search<T>(IEnumerable<T> start, Func<T, IEnumerable<T>> following, Predicate<T> match)
        {
            var front = start.Select(x => new Node<T> { Value = x }).ToList();

            var expaneded = new List<Node<T>>();
            while (front.Count != 0)
            {
                var item = front[0];
                if (!expaneded.Contains(item))
                {
                    if (match(item.Value))
                    {
                        var resultList = new List<T>();
                        do
                        {
                            resultList.Add(item.Value);
                            item = item.Parent;
                        }
                        while (item != null);
    
                        resultList.Reverse();
                        return resultList.ToArray();
                    }

                    expaneded.Add(item);
                    front.AddRange(following(item.Value).Select(x => new Node<T> { Value = x, Parent = item }));
                }

                front.RemoveAt(0);
            }

            return null;
        }
        
        class Node<T>
        {
            public Node<T> Parent;
            public T Value;
        }
    }
}
