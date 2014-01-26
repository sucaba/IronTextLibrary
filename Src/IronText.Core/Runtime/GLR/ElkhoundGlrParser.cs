#if ELKHOUND
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Automata.Lalr1;
using System.Diagnostics;
using IronText.Framework;

namespace IronText.Runtime
{
    using State = System.Int32;
    using RuleIndex = System.Int32;
    using IronText.Diagnostics;

    // Simplistic Elkhound parsing algorinthm from paper
    sealed class ElkhoundGlrParser 
        : IPushParser
        , IParsing 
    {
        private static readonly Msg Eoi = new Msg { Id = BnfGrammar.Eoi };

        private readonly Gss gss;
        private readonly Queue<GssReducePath> worklist = new Queue<GssReducePath>(4);
        private bool accepted = false;

        private readonly BnfGrammar            grammar;
        private readonly object                context;
        private readonly int[]                 conflictActionsTable;
        private readonly int[]               stateToPriorToken;
        private readonly TransitionDelegate    transition;
        private readonly GrammarActionDelegate grammarAction;

        public ElkhoundGlrParser(
            object                  context,
            BnfGrammar              grammar,
            TransitionDelegate      transition,
            GrammarActionDelegate   grammarAction,
            int[]                 stateToPriorToken,
            int[]                   conflictActionsTable,
            Func<object, int, IReciever<Msg>, IReciever<Msg>> makeSwitch)
        {
            this.context              = context;
            this.grammar              = grammar;
            this.grammarAction        = grammarAction;
            this.transition           = transition;
            this.stateToPriorToken    = stateToPriorToken;
            this.conflictActionsTable = conflictActionsTable;
            this.gss                  = new Gss();

            gss.AddTopmost(0);
        }

        public bool CanRecieve { get { return true; } }

        public IReciever<Msg> Next(Msg item)
        {
            Reducer(item); 

            Shifter(item);

#if DIAGNOSTICS
            using (var graphView = new GvGraphView("GlrState" + gss.CurrentLayer + ".gv"))
            {
                gss.WriteGraph(graphView, grammar, stateToPriorToken);
            }
#endif

            if (gss.IsEmpty)
            {
                throw new SyntaxException(
                        null,
                        item.Location,
                        "Unexpected token " + grammar.TokenName(item.Id));
            }

            return this;
        }

        public IReciever<Msg> Done()
        {
            Reducer(Eoi);
            Shifter(Eoi);

#if DIAGNOSTICS
            using (var graphView = new GvGraphView("GlrState" + gss.CurrentLayer + (accepted ? "Accept" : "Fail") + ".gv"))
            {
                gss.WriteGraph(graphView, grammar, stateToPriorToken);
            }
#endif

            // TODO: Remove non-accepting topmost nodes.

            if (!accepted)
            {
                throw new SyntaxException(null, Loc.Unknown, "Unexpected EOI");
            }

            return this;
        }

        public IReciever<Msg> Fail(Exception providerFailure)
        {
            throw new NotImplementedException();
        }

        private void EnqueueReductionPaths(GssStateNode v, Reduction red, GssStateLink mustUseLink)
        {
            var size = red.Rule.Parts.Length;

            foreach (GssReducePath path in gss.GetReducePaths(v, size))
            {
                if (mustUseLink != null && (size == 0 || path.RightmostLink != mustUseLink))
                {
                    continue;
                }

                path.LeftmostStateNode = size == 0 ? v : path.First.Link.PreviousState;
                path.Size = size;
                path.Rule = red.Rule;
                worklist.Enqueue(path);
            }
        }

        private void Reducer(Msg item)
        {
            foreach (var node in gss.Topmost)
            {
                foreach (var red in GetReductions(node.State, item.Id))
                {
                    EnqueueReductionPaths(node, red, null);
                }
            }

            while (worklist.Count != 0)
            {
                var path = worklist.Dequeue();
                ReduceViaPath(path, item);
            }
        }

        private void ReduceViaPath(GssReducePath path, Msg item)
        {
            int N = path.Rule.Left;
            GssStateNode leftSib = path.LeftmostStateNode;
            State k = leftSib.State;

            // TODO: Apply 'Duplicate' to all path semantic values
            Msg newItem = InvokeReduceAction(path, item);
            int newState = NonTermGoTo(k, N);
            var rightSib = gss.FindTopmost(newState);

            if (rightSib != null)
            {
                var link = gss.GetLink(rightSib, leftSib);
                if (link != null)
                {
                    link.Item = MergeValues(newState, link.Item, newItem);
                }
                else
                {
                    link = gss.AddLink(rightSib, leftSib, newItem);
                    EnqueueLimitedReductions(item.Id, link);
                }
            }
            else
            {
                rightSib = gss.AddTopmost(newState);
                var link = gss.AddLink(rightSib, leftSib, newItem);
                EnqueueLimitedReductions(item.Id, link);
            }
        }

        /// <summary>
        /// Enqueue all reductions that use newly-created link
        /// </summary>
        private void EnqueueLimitedReductions(int lookahead, GssStateLink link)
        {
            foreach (var n in gss.Topmost)
            {
                foreach (Reduction red in GetReductions(n.State, lookahead))
                {
                    EnqueueReductionPaths(n, red, link);
                }
            }
        }

        private void Shifter(Msg item)
        {
            var prevTops = gss.Topmost.ToArray();
            gss.PushLayer();

            foreach (var current in prevTops)
            {
                int? shift;
                if (GetParserActions(current.State, item.Id, out shift, null) && shift.HasValue)
                {
                    var rightSib = gss.FindTopmost(shift.Value);
                    if (rightSib != null)
                    {
                        gss.AddLink(rightSib, current, item);
                    }
                    else
                    {
                        rightSib = gss.AddTopmost(shift.Value);
                        gss.AddLink(rightSib, current, item);
                    }
                }
            }
        }
        
        private Msg MergeValues(State state, Msg oldItem, Msg newItem)
        {
            return newItem;
        }

        private Msg InvokeReduceAction(GssReducePath path, Msg currentItem)
        {
            var rule = path.Rule;
            // TODO: Fix inconsistency? Size of "modified reduction" path can be shorter.
            var args = new Msg[rule.Parts.Length];
            var node = path.First;
            int i = 0;
            var loc = Loc.Unknown;
            if (node == null)
            {
                loc = new Loc(currentItem.Location.FilePath, currentItem.Location.Position, 0);
            }
            else
            {
                do
                {
                    var item = node.Link.Item;
                    args[i] = item;
                    loc += item.Location;

                    node = node.Next;
                    ++i;
                }
                while (node != null);
            }

            var value = grammarAction(rule, args, 0, context, path.LeftmostStateNode);
            return new Msg(rule.Left, value, loc);
        }
        
        private int NonTermGoTo(State state, int token)
        {
            var action = ParserAction.Decode(transition(state, token));
            if (action == null || action.Kind != ParserActionKind.Shift)
            {
                throw new InvalidOperationException("Non-term action should be shift");
            }

            return action.State;
        }

        private IEnumerable<Reduction> GetReductions(State state, int token)
        {
            var result = new List<Reduction>();
            int? shift;
            GetParserActions(state, token, out shift, result);
            return result;
        }

        private bool GetParserActions(State state, int token, out int? shift, List<Reduction> reductions)
        {
            shift = null;

            ParserAction action = GetDfaCell(state, token);
            if (action == null)
            {
                return false;
            }

            switch (action.Kind)
            {
                case ParserActionKind.Shift:
                    shift = action.State;
                    break;
                case ParserActionKind.Reduce:
                    if (reductions != null)
                    {
                        var rule = grammar.Rules[action.Rule];
                        reductions.Add(new Reduction(rule));
                    }
                    break;
                case ParserActionKind.Accept:
                    accepted = true;
                    break;
                case ParserActionKind.Conflict:
                    foreach (ParserAction conflictAction in GetConflictActions(action.Value1, action.Value2))
                    {
                        switch (conflictAction.Kind)
                        {
                            case ParserActionKind.Shift:
                                shift = conflictAction.State;
                                break;
                            case ParserActionKind.Reduce:
                                if (reductions != null)
                                {
                                    var crule = grammar.Rules[conflictAction.Rule];
                                    reductions.Add(new Reduction(crule));
                                }
                                break;
                        }
                    }

                    break;
            }

            return true;
        }

        private IEnumerable<ParserAction> GetConflictActions(int start, short count)
        {
            int last = start + count;
            while (start != last)
            {
                yield return ParserAction.Decode(conflictActionsTable[start++]);
            }
        }

        private ParserAction GetDfaCell(State state, int token)
        {
            int cell = transition(state, token);
            var result = ParserAction.Decode(cell);
            return result;
        }

        struct Reduction
        {
            public Reduction(BnfGrammar.Rule rule)
            {
                this.Rule = rule;
            }

            public readonly BnfGrammar.Rule Rule;
        }

        Loc IParsing.Location
        {
            get { throw new NotImplementedException(); }
        }
    }

}
#endif
