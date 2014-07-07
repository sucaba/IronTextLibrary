using IronText.Collections;
using IronText.Reflection;
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

        public GrammarTransformationSteps()
        {
            this.grammar = new Grammar { StartName = StartSymbolName };
        }

        [Given(@"used symbol '(\w+)'")]
        public void GivenUsedSymbol(string symbolName)
        {
            GivenProduction(StartSymbolName, new [] { symbolName });
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

        [Given(@"production duplicate resolver '(.*)'")]
        public void GivenProductionDuplicateResolver(string resolverName)
        {
            grammar.Productions.DuplicateResolver = DuplicateResolver<Production>.ByName(resolverName);
        }

        [When(@"decompose symbol '(\w+)' from symbol '(\w+)'")]
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

        [When(@"safe adding production '(\w+) =(.*)'")]
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

        [Then(@"result symbol is '(\w+)'")]
        public void ThenResultSymbolIs(string symbolName)
        {
            Assert.AreEqual(symbolName, resultSymbol.Name);
        }

        [Then(@"'(\w+)' has (\d+) production[s]?")]
        public void ThenSymbolHasProductionCount(string symbolName, int count)
        {
            Assert.AreEqual(count, grammar.Symbols[symbolName].Productions.Count);
        }

        [Then(@"production exists '(\w+) =(.*)'")]
        public void ThenProductionExist(string outcome, string[] pattern)
        {
            Assert.IsTrue(null != grammar.Productions.Find(outcome, pattern));
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
            Symbol found = grammar.Symbols.FirstOrDefault(s => s.Name == symbolName) as Symbol;
            Assert.IsNotNull(found != null);
            Assert.IsFalse(found.IsUsed);
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

        [StepArgumentTransformation]
        public static string[] ParseSymbols(string pattern)
        {
            return pattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
