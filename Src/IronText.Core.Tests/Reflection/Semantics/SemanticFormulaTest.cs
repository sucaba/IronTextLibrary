using IronText.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IronText.Tests.Reflection.Semantics
{
    [TestFixture]
    public class SemanticFormulaTest
    {
        [Datapoint]
        public readonly SemanticVariable InhLhe = new SemanticVariable("<lhe inherited>", 1);

        [Datapoint]
        public readonly SemanticVariable SynthLhe = new SemanticVariable("<lhe synth>", 0);

        [Datapoint]
        public readonly SemanticReference InhRhe = new SemanticReference("<rhe inherited>", 0);

        [Datapoint]
        public readonly SemanticReference SynthRhe = new SemanticReference("<rhe synth>", 1);

        [Theory]
        public void Test(SemanticVariable lhe, SemanticReference rhe)
        {
            var sut = new SemanticFormula(lhe, rhe);
            Assert.IsTrue(sut.IsCopy);
        }


        [Test]
        public void InhLheNegativeTest()
        {
            Expression<Func<object, object, object>> e = (x, y) => null;
            var sut = new SemanticFormula(
                            InhLhe,
                            new[] { InhRhe, InhRhe },
                            e);
            Assert.IsFalse(sut.IsCopy);
        }

        [Test]
        public void SynthLheNegativeTest()
        {
            Expression<Func<object, object, object>> e = (x, y) => null;
            var sut = new SemanticFormula(
                            SynthLhe,
                            new[] { SynthRhe, SynthRhe },
                            e);
            Assert.IsFalse(sut.IsCopy);
        }
    }
}
