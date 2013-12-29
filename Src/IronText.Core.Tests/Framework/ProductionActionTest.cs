using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Reflection;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class ProductionActionTest
    {
        [Test]
        public void Test()
        {
            var target = new ProductionAction(0, 3);
        }

#if false
        class CompositeProductionAction
        {
        }

        [Test]
        public void InliningBuildAddsActions(CompositeProductionAction action)
        {
            var target = new CompositeProductionAction();
            // [A B C D E]
            target.AddReduction(new ProductionAction(1, 2));
            // [A F D E]
            target.AddReduction(new ProductionAction(0, 3));
            // [G E]
            target.AddReduction(new ProductionAction(0, 2));
            // [H] : Done
            
            // Invariant: 5 = 1 + (2 - 1) + (3 - 1) + (2 - 1)
            Assert.AreEqual(5, target.Size);
        }
#endif
    }
}
