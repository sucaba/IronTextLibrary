using IronText.Collections;
using System;

namespace IronText.Reflection
{
    [Serializable]
    public class MatcherCollection : IndexedCollection<Matcher,IGrammarScope>
    {
        public MatcherCollection(IGrammarScope context)
            : base(context)
        {
        }

        public Matcher Add(string literal, Disambiguation? disambiguation = null)
        {
            var outcomeSymbol = Scope.Symbols.ByName("'" + literal + "'");
            var result = new Matcher(ScanPattern.CreateLiteral(literal), outcomeSymbol, disambiguation);
            return Add(result);
        }

        public Matcher Add(string outcome, string pattern, Disambiguation? disambiguation = null)
        {
            var outcomeSymbol = Scope.Symbols.ByName(outcome);
            var result = new Matcher(ScanPattern.CreateRegular(pattern), outcomeSymbol, disambiguation);
            return Add(result);
        }
    }
}
