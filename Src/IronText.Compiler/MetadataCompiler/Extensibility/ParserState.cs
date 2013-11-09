using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using System.Collections.ObjectModel;
using IronText.Framework;
using IronText.Automata.Lalr1;
using IronText.Framework.Reflection;

namespace IronText.MetadataCompiler
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
                                    dotState.Items.ToArray());
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
                    var list = new List<IParserTransition>();
                    int tokenCount = data.TokenCount;
                    for (int token = EbnfGrammar.Eoi; token != tokenCount; ++token)
                    {
                        var actions = GetAllParserActions(dotState.Index, token);
                        if (actions.Count() != 0)
                        {
                            list.Add(
                                new ParserTransition(
                                    token,
                                    actions));
                        }
                    }

                    transitions = new ReadOnlyCollection<IParserTransition>(list);
                }

                return transitions;
            }
        }

        private IEnumerable<ParserAction> GetAllParserActions(int state, int token)
        {
            var cell = data.ParserActionTable.Get(state, token);
            var action = ParserAction.Decode(cell);
            if (action == null || action.Kind == ParserActionKind.Fail)
            {
            }
            else if (action.Kind == ParserActionKind.Conflict)
            {
                for (int i = 0; i != action.Size; ++i)
                {
                    yield return
                        ParserAction.Decode(
                            data.ParserConflictActionTable[action.Value1 + i]);
                }
            }
            else
            {
                yield return action;
            }
        }
    }
}
