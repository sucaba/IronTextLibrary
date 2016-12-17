using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronText.Runtime;

namespace IronText.Automata.TurnPlanning
{
    class TurnConflictResolver
    {
        private readonly TurnPrecedenceProvider precedenceProvider;

        public TurnConflictResolver(TurnPrecedenceProvider precedenceProvider)
        {
            this.precedenceProvider = precedenceProvider;
        }

        public bool TryResolve(
            ParserDecision current,
            ParserDecision decision,
            int            token,
            out ParserDecision resolved)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> PrioritizeBy<T>(IEnumerable<T> items, Func<T,Turn> getTurn)
        {
            // TODO: Handle non-associative operators
            return items.OrderByDescending(x => GetWeight(getTurn(x)));
        }

        private int GetWeight(Turn turn)
        {
            var p = precedenceProvider.GetPrecedence(turn);

            return p.Value * 2 + (p.Assoc == Reflection.Associativity.Left ? 1 : 0);
        }
    }
}
