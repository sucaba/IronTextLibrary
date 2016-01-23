using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Algorithm
{
    internal static class Graph
    {
#if false
        public static List<List<T>> Scc<T>(IEnumerable<T> startItems, Func<T, IEnumerable<T>> following)
            where T : IEquatable<T>
        {
            var tarjan = new TarjanSccAlgorithm<T>(startItems, following);
            return tarjan.Result;
        }
#endif
        public static List<T> TopologicalSort<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> following)
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

        public static T[] BreadthFirst<T>(T start, Func<T, IEnumerable<T>> following)
        {
            return BreadthFirst(start, following, EqualityComparer<T>.Default);
        }

        public static T[] BreadthFirst<T>(T start, Func<T, IEnumerable<T>> following, IEqualityComparer<T> cmp)
        {
            return BreadthFirst(new[] { start }, following, cmp);
        }

        public static T[] BreadthFirst<T>(IEnumerable<T> startItems, Func<T, IEnumerable<T>> following)
        {
            return BreadthFirst(startItems, following, EqualityComparer<T>.Default);
        }

        public static T[] BreadthFirst<T>(IEnumerable<T> startItems, Func<T, IEnumerable<T>> following, IEqualityComparer<T> cmp)
        {
            var result = new List<T>();
            AddBreadthFirst(startItems, following, result, cmp);
            return result.ToArray();
        }

        public static void AddBreadthFirst<T>(IEnumerable<T> startItems, Func<T, IEnumerable<T>> following, List<T> output, IEqualityComparer<T> cmp)
        {
            output.AddRange(startItems);
            AddBreadthFirst(following, output, 0, cmp);
        }

        public static void AddBreadthFirst<T>(Func<T, IEnumerable<T>> following, List<T> output, int frontIndex)
        {
            AddBreadthFirst(following, output, frontIndex, EqualityComparer<T>.Default);
        }

        public static void AddBreadthFirst<T>(Func<T, IEnumerable<T>> following, List<T> output, int frontIndex, IEqualityComparer<T> cmp)
        {
            while (frontIndex != output.Count)
            {
                var item = output[frontIndex++];
                var f = following(item);
                f = f.Except(output, cmp);
                output.AddRange(f);
            }
        }

        public static T[] BreadthFirstSearch<T>(T start, Func<T, IEnumerable<T>> following, Predicate<T> match)
        {
            return BreadthFirstSearch(new [] { start }, following, match);
        }

        /// <summary>
        /// Finds path from the <paramref name="start"/> to the first element in 
        /// a left-to-right, top-down order which matches <paramref name="match"/> predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start"></param>
        /// <param name="following"></param>
        /// <param name="match"></param>
        /// <returns>
        /// Path of nodes starting from the first one to the <i>found</i> one or 
        /// <c>null</c> if path was not found.
        /// </returns>
        public static T[] BreadthFirstSearch<T>(IEnumerable<T> start, Func<T, IEnumerable<T>> following, Predicate<T> match)
        {
            var front = start.Select(x => new Node<T> { Value = x }).ToList();

            var expaneded = new List<T>();
            while (front.Count != 0)
            {
                var item = front[0];
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

                expaneded.Add(item.Value);
                var followingValues = following(item.Value).Except(expaneded);
                front.AddRange(followingValues.Select(x => new Node<T> { Value = x, Parent = item }));

                front.RemoveAt(0);
            }

            return null;
        }

        /// <summary>
        /// Clustering for undirected graph
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="all"></param>
        /// <param name="following"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static List<List<T>> Cluster<T>(
            IEnumerable<T>         all,
            Func<T,IEnumerable<T>> following,
            IEqualityComparer<T>   comparer = null)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            var result = new List<List<T>>();
            foreach (var v in all)
            {
                if (result.Any(c => c.Contains(v)))
                {
                    continue;
                }

                result.Add(TopologicalSort(new[] { v }, following));
            }

            return result;
        }
        
        class Node<T>
        {
            public Node<T> Parent;
            public T Value;
        }
    }
}
