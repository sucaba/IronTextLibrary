using IronText.Automata.Lalr1;
using System;

namespace IronText.MetadataCompiler
{
    class StateToSymbolTableProvider
    {
        public StateToSymbolTableProvider(ILrDfa lrDfa)
        {
            Table = Array.ConvertAll(lrDfa.States, GetStateToken);
        }

        public int[] Table { get; }

        static int GetStateToken(DotState state)
        {
            foreach (var item in state.Items)
            {
                int token = item.PreviousToken;
                if (token != -1)
                {
                    return token;
                }
            }

            return -1;
        }
    }
}
