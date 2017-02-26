using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IronText.Reporting
{
    public interface IParserAutomata
    {
        ReadOnlyCollection<IParserState>       States    { get; }
    }

    public static class ParserAutomataExtensions
    {
        public static IEnumerable<ParserConflict> GetConflicts(this IParserAutomata automata)
        {
            return automata
                .States
                .SelectMany(s => s
                    .Transitions
                    .Where(t => t.AlternateDecisions.Skip(1).Any())
                    .Select(t => new ParserConflict(s, t)));
        }
    }
}