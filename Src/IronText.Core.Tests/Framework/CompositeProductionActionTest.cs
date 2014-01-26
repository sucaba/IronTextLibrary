using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Reflection;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class CompositeProductionActionTest
    {
        [Test]
        public void HasCorrectArgumentCountTest()
        {
            var target = new CompositeProductionAction();
            // [A B C D E]
            target.Subactions.Add(new SimpleProductionAction(1, 2));
            // [A F D E]
            target.Subactions.Add(new SimpleProductionAction(0, 3));
            // [G E]
            target.Subactions.Add(new SimpleProductionAction(0, 2));
            // [H] : Done
            
            // Invariant: 5 = 1 + (2 - 1) + (3 - 1) + (2 - 1)
            Assert.AreEqual(5, target.ArgumentCount);
        }
    }
}
