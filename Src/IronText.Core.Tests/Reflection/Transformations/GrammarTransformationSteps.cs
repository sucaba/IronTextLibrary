using IronText.Collections;
using IronText.Reflection;
using IronText.Reflection.Transformations;
using NUnit.Framework;
using System;
using System.Linq;
using TechTalk.SpecFlow;

namespace IronText.Tests.Reflection.Transformations
{
    [Binding]
    public class GrammarTransformationSteps
    {
        private const string StartSymbolName = "Start";

        private readonly Grammar      grammar;
        private Func<Production,bool> criteria;
        private Symbol                resultSymbol;
        private Symbol[]              resultSymbols;
        private Exception             resultException;
        private bool                  resultBool;

        public GrammarTransformationSteps()
        {
            this.grammar = new Grammar { StartName = StartSymbolName };
        }

        [Given(@"used symbol '([^']+)'")]
        public void GivenUsedSymbol(string symbolName)
        {
            GivenProduction(StartSymbolName, new [] { symbolName });
        }

        [Given(@"production '([^ =]+) =(.*)'")]
        public void GivenProduction(string outcome, string[] pattern)
        {
            grammar.Productions.Add(outcome, pattern);
        }

        [Given(@"symbol '(.*)'")]
        public void GivenSymbol(string name)
        {
            grammar.Symbols.Add(name);
        }

        [Given(@"production criteria is: input has '([^']+)' symbol")]
        public void GivenProductionCriteriaTheInputHasSymbol(string symbolName)
        {
            this.criteria = p => p.Input.Any(s => s.Name == symbolName);
        }

        [Given(@"production criteria is: input is not empty")]
        public void GivenProductionCriteriaIsInputIsNotEmpty()
        {
            this.criteria = p => p.Input.Length != 0;
        }

        [Given(@"production duplicate resolver '(.*)'")]
        public void GivenProductionDuplicateResolver(string resolverName)
        {
            grammar.Productions.DuplicateResolver = DuplicateResolver<Production>.ByName(resolverName);
        }

        [When(@"detect if symbol '(.*)' is recursive")]
        public void WhenDetectIfSymbolIsRecursive(string symbol)
        {
            resultBool = grammar.Symbols.ByName(symbol).IsRecursive;
        }

        [When(@"decompose symbol '([^']+)' from symbol '([^']+)'")]
        public void WhenDecomposeSymbolFromSymbol(string toSymbol, string fromSymbol)
        {
            this.resultSymbol = grammar.Decompose(grammar.Symbols[fromSymbol], criteria, toSymbol);
        }

        [When(@"inline grammar")]
        public void WhenInlineGrammar()
        {
            grammar.Inline();
        }

        [When(@"find opt symbols")]
        public void WhenFindOptionalPatternSymbols()
        {
            this.resultSymbols = grammar.FindOptionalPatternSymbols();
        }

        [When(@"inline opt symbols")]
        public void WhenInlineOptionalSymbols()
        {
            grammar.InlineOptionalSymbols();
        }

        [When(@"find nullable non-opt symbols")]
        public void WhenFindNullableNonOptSymbols()
        {
            this.resultSymbols = grammar.FindNullableNonOptSymbols().ToArray();
        }

        [When(@"convert nullable, non-opt symbols into opt")]
        public void WhenConvertNullableSymbolsIntoOpt()
        {
            grammar.ConvertNullableNonOptToOpt();
        }

        [When(@"eliminate empty productions")]
        public void WhenEliminateEmptyProductions()
        {
            grammar.EliminateEmptyProductions();
        }

        [When(@"recursively eliminate empty productions")]
        public void WhenRecursivelyEliminateEmptyProductions()
        {
            grammar.RecursivelyEliminateEmptyProductions();
        }

        [When(@"safe adding production '([^= ]+) =(.*)'")]
        public void WhenAddingProduction(string outcome, string[] pattern)
        {
            try
            {
                grammar.Productions.Add(outcome, pattern);
            }
            catch (AssertionException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.resultException = e;
            }
        }

        [When(@"eliminate right nullable symbols")]
        public void WhenEliminateRightNullableSymbols()
        {
            var transformation = new EliminateRightNulls(grammar);
            transformation.Apply();
        }

        [Then(@"result symbol is '([^']+)'")]
        public void ThenResultSymbolIs(string symbolName)
        {
            Assert.AreEqual(symbolName, resultSymbol.Name);
        }

        [Then(@"'([^']+)' has (\d+) production[s]?")]
        public void ThenSymbolHasProductionCount(string symbolName, int count)
        {
            Assert.AreEqual(count, grammar.Symbols[symbolName].Productions.Count);
        }

        [Then(@"production exists '([^']*)'")]
        public void ThenProductionExist(string productionText)
        {
            var prod = grammar.Productions.Find(productionText);
            if (prod == null)
            {
                Assert.Fail("Production '{0}' not found.", productionText);
            }
        }

        [Then(@"'(.*)' is identity production")]
        public void ThenIsIdentityProduction(string productionText)
        {
            var prod = grammar.Productions.Find(productionText);
            Assert.IsTrue(prod.HasIdentityAction);
        }

        [Then(@"result symbols are '(.*)'")]
        public void ThenResultSymbolsAre(string[] symbols)
        {
            Assert.That(resultSymbols.Select(s => s.Name), Is.EquivalentTo(symbols));
        }

        [Then(@"symbol exists '(.*)'")]
        public void ThenSymbolExists(string symbolName)
        {
            grammar.Symbols.Any(s => s.Name == symbolName);
        }

        [Then(@"symbol '(.*)' is not used")]
        [Then(@"symbol '(.*)' is unused")]
        public void ThenSymbolIsNotUsed(string symbolName)
        {
            var found = grammar.Symbols.FirstOrDefault(s => s.Name == symbolName);
            Assert.IsNotNull(found);
            Assert.IsFalse(found.IsUsed);
        }

        [Then(@"symbol '(.*)' is used")]
        public void ThenSymbolIsUsed(string symbolName)
        {
            var found = grammar.Symbols.FirstOrDefault(s => s.Name == symbolName);
            Assert.IsNotNull(found);
            Assert.IsTrue(found.IsUsed);
        }

        [Then(@"result exception is '(.*)'")]
        public void ThenResultExceptionIs(string exceptionTypeName)
        {
            var exceptionType = Type.GetType(exceptionTypeName);
            Assert.IsNotNull(resultException);
            Assert.That(resultException, Is.InstanceOf(exceptionType));
        }

        [Then(@"no result exception caught")]
        public void ThenNoResultExceptionCaught()
        {
            Assert.IsNull(resultException);
        }

        [Then(@"result should be '(.*)'")]
        public void ThenResultShouldBe(string flag)
        {
            bool expected = bool.Parse(flag);
            Assert.AreEqual(expected, resultBool);
        }

        [StepArgumentTransformation]
        public static string[] ParseSymbols(string pattern)
        {
            return pattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
