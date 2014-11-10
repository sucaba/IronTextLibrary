using IronText.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Reflection
{
    [TestFixture]
    public class ProductionSemanticActionTest
    {
        [Test]
        public void AddCopyingTest()
        {
            var grammar = new Grammar {
                Productions = { 
                    { "S", new [] { "X" } },
                },
                Matchers = {
                    { "X", "'x'" }
                }
            };

            grammar.SemanticActions.Add("S = X", "S?fromProp", "X.toProp");

            Assert.AreSame(grammar.Productions.Find("S = X"), grammar.SemanticActions.Single().Production);
        }
    }
}
