using IronText.Reflection;
using NUnit.Framework;
using System;
using System.Linq;
using TechTalk.SpecFlow;

namespace IronText.Tests.Reflection.Transformations
{
    [Binding]
    public class DecomposeTransformationSteps
    {
        private readonly Grammar grammar;
        private Func<Production,bool> criteria;
        private Symbol resultSymbol;

        public DecomposeTransformationSteps()
        {
            this.grammar = new Grammar();
        }

        [Given(@"production '(\w+) =(.*)'")]
        public void GivenProduction(string outcome, string[] pattern)
        {
            grammar.Productions.Add(outcome, pattern);
        }

        [Given(@"production criteria is: input has '(\w+)' symbol")]
        public void GivenProductionCriteriaTheInputHasSymbol(string symbolName)
        {
            this.criteria = p => p.Pattern.Any(s => s.Name == symbolName);
        }

        [Given(@"production criteria is: input is not empty")]
        public void GivenProductionCriteriaIsInputIsNotEmpty()
        {
            this.criteria = p => p.Pattern.Length != 0;
        }

        [When(@"decompose symbol '(\w+)' from symbol '(\w+)'")]
        public void WhenDecomposeSymbolFromSymbol(string toSymbol, string fromSymbol)
        {
            this.resultSymbol = grammar.Decompose(grammar.Symbols[fromSymbol], criteria, toSymbol);
        }

        [Then(@"result symbol is '(\w+)'")]
        public void ThenResultSymbolIs(string symbolName)
        {
            Assert.AreEqual(symbolName, resultSymbol.Name);
        }

        [Then(@"'(\w+)' has (\d+) productions")]
        public void ThenSymbolHasProductionCount(string symbolName, int count)
        {
            Assert.AreEqual(count, grammar.Symbols[symbolName].Productions.Count);
        }

        [Then(@"production exists '(\w+) =(.*)'")]
        public void ThenProductionExist(string outcome, string[] pattern)
        {
            Assert.IsNotNull(grammar.Productions.Find(outcome, pattern));
        }

        [StepArgumentTransformation]
        public static string[] ParseSymbols(string pattern)
        {
            return pattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
