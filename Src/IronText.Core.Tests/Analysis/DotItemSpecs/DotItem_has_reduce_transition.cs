#if false
using NUnit.Framework;
using IronText.Automata;
using IronText.Automata.DotNfa;
using IronText.Algorithm;
using System;
using IronText.Automata.TreeNfa;

namespace IronText.Tests.Analysis.DotItemSpecs
{
    [TestFixture]
    public class DotItem_has_reduce_transition
    {
        const int S = 10;
        const int X = 11;
        const int Y = 12;
        const int Z = 13;
        const int T = 14;
        const int P = 15;
        const int E = 20;
        const int ContainerProductionId     = 100;
        const int TopInlinedProductionId    = 101;
        const int Child1InlinedProductionId = 102;
        const int Child2InlinedProductionId = 103;

        [Test]
        public void Test()
        {
            // X = (Y = Z .) E [<any>] 

            var y = TreeNode.Reduction(
                    Child2InlinedProductionId,
                    Y,
                    TreeNode.ReadInput(Z));

            var root = 
                TreeNode.Reduction(
                    TopInlinedProductionId,
                    X,
                    y,
                    TreeNode.ReadInput(E)); 

            var sut = new TreeItem(root);

            Assert.That(
                sut.AllTransitions, 
                Has
                .Exactly(1)
                .Matches((DotItemTransition t) => IsReduction(t, Child2InlinedProductionId)));
        }

        private bool IsReduction(DotItemTransition t, int productionId)
        {
            var asReduction = t as DotItemReduceTransition;
            return asReduction != null & asReduction.ProductionId == productionId;
        }
    }
}
#endif
