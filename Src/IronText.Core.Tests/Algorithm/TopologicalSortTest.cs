using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class TopologicalSortTest
    {
        private const int A = 0, B = 1, C = 2, D = 3, E = 4, F = 5, count = F + 1;
        private static string[] names = { "A", "B", "C", "D", "E", "F" };

        [Test]
        public void UncycledTest()
        {
            int[][] following = new int[count][];

            following[A] = new [] { B, D };
            following[B] = new [] { C };
            following[C] = new [] { D, E };
            following[D] = new [] { E };
            following[E] = new int[0];

            var result = Graph.TopologicalSort(new [] { A }, n => following[n]).ToArray();
            Debug.Write(string.Join(", ", result.Select(n => names[n])));
        }

        [Test]
        public void CycledTest()
        {
            int[][] following = new int[count][];

            following[A] = new [] { B, D };
            following[B] = new [] { C };
            following[C] = new [] { D };
            following[D] = new [] { E };
            following[E] = new [] { C };
            following[E] = new [] { F };
            following[F] = new int[0];

            var result = Graph.TopologicalSort(new [] { A }, n => following[n]).ToArray();
            Debug.Write(string.Join(", ", result.Select(n => names[n])));
        }
    }
}
