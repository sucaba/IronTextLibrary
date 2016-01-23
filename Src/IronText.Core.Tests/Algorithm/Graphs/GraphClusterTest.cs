using IronText.Algorithm;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Algorithm.Graphs
{
    [TestFixture]
    public class GraphClusterTest
    {
        [Test]
        public void TwoConnectedVertexesAreClusteredTest()
        {
            string[] all = { "v1", "v2" };
            var clusters = Graph.Cluster(all, v => v == "v1" ? new[] { "v2" } : new string[0]);
            Assert.AreEqual(1, clusters.Count);
            CollectionAssert.AreEquivalent(new [] { "v1", "v2" }, clusters[0]);
        }

        [Test]
        public void TwoDisconnectedVertexesAreInDifferentClustersTest()
        {
            string[] all = { "v1", "v2" };
            var clusters = Graph.Cluster(all, v => new string[0]);
            Assert.AreEqual(2, clusters.Count);
            CollectionAssert.AreEquivalent(new [] { "v1" }, clusters[0]);
            CollectionAssert.AreEquivalent(new [] { "v2" }, clusters[1]);
        }
    }
}
