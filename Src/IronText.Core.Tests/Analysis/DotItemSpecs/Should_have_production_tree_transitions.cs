#if false
using IronText.Compiler.Analysis;
using IronText.Reflection;
using IronText.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Analysis.DotItemSpecs
{
    [TestFixture]
    public class Should_have_production_tree_transitions
    {
        const int S = 10;
        const int X = 11;

        [Test]
        public void Test()
        {
            var parent = new RuntimeProduction(
                0,
                S,
                new[] { X },
                new RuntimeProductionNode(
                    S,
                    )); 
            var sut = new DotItem();
        }
    }
}
#endif
