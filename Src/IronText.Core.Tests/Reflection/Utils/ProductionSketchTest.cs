using IronText.Reflection.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PS = IronText.Reflection.Utils.ProductionSketch;

namespace IronText.Tests.Reflection.Utils
{
    [TestFixture]
    public class ProductionSketchTest
    {
        [Test]
        public void OutcomeIsParsedCorrectly()
        {
            Assert.AreEqual("A", GetOutcome("A = B C"));
            Assert.AreEqual("A", GetOutcome("A : B C"));
        }

        [Test]
        public void FlatComponentsAreParsedCorrectly()
        {
            Assert.AreEqual(new [] { "B", "C" }, GetComponents("A = B C"));
            Assert.AreEqual(new [] { "B" }, GetComponents("A = B"));
            Assert.AreEqual(new [] { "B" }, GetComponents("A=B"));
            Assert.AreEqual(new object[0], GetComponents("A = "));
        }

        [Test]
        public void InlinedComponentIsParsedCorrectly()
        {
            var sketch = PS.Parse("A = (B = C)");
            Assert.AreEqual(
                new PS("A", new PS("B", "C")),
                sketch);
        }

        [Test]
        public void InlinedComponentsAreParsedCorrectly()
        {
            Assert.AreEqual(
                new PS("A", "Pre", new PS("B", "C"), "Post"), 
                PS.Parse("A = Pre (B = C) Post"));

            Assert.AreEqual(
                new PS("A", "Pre", new PS("B"), "Post"), 
                PS.Parse("A = Pre (B=) Post"));

            Assert.AreEqual(
                new PS("A", "Pre", new PS("B"), "Post"), 
                PS.Parse("A=Pre(B=)Post"));
        }

        public string GetOutcome(string text)
        {
            ProductionSketch sketch = ProductionSketch.Parse("A = B C");
            return sketch.Outcome;
        }

        private object[] GetComponents(string text)
        {
            ProductionSketch sketch = ProductionSketch.Parse(text);
            return sketch.Components;
        }
    }
}
