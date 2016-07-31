﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IronText.Automata.Lalr1;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Runtime;
using IronText.Collections;

namespace IronText.Reflection.Reporting
{
    class ParserState : IParserState
    {
        private readonly LanguageData data;
        private readonly DotState dotState;
        private ReadOnlyCollection<IParserDotItem> items;
        private ReadOnlyCollection<IParserTransition> transitions;

        public ParserState(DotState dotState, LanguageData data)
        {
            this.dotState = dotState;
            this.data     = data;
        }

        public int Index { get { return dotState.Index; } }

        public ReadOnlyCollection<IParserDotItem> DotItems
        {
            get 
            {
                if (items == null)
                {
                    items = new ReadOnlyCollection<IParserDotItem>(
                                    dotState.Items
                                        .Select(it => 
                                            new ParserDotItem(data.Grammar.Productions[it.ProductionId], it.Position, it.LA))
                                        .ToArray());
                }

                return items; 
            }
        }

        public ReadOnlyCollection<IParserTransition> Transitions
        {
            get 
            {
                if (transitions == null)
                {
                    var list = data.Grammar
                        .Symbols
                        .Select(symbol => (IParserTransition)
                            new ParserTransition(
                                symbol.Index,
                                data.ParserActionTable.Get(
                                    dotState.Index,
                                    symbol.Index)))
                        .ToList();

                    transitions = new ReadOnlyCollection<IParserTransition>(list);
                }

                return transitions;
            }
        }

        private IEnumerable<ParserDecision> GetAllParserActions(int state, int token)
        {
            var decision = data.ParserActionTable.Get(state, token);
            return decision.Alternatives();
        }
    }
}
