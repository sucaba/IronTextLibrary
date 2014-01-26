using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using IronText.Reflection;

namespace IronText.Framework
{
    class DeterministicParser<TNode> : IPushParser
    {
        private const int InitialValueStackSize = 32;

        private readonly RuntimeEbnfGrammar     grammar;
        private readonly TransitionDelegate     actionTable;

        private readonly TaggedStack<TNode> stateStack;

        private readonly Production[] rules;
        private Production            currentRule;
        private IProducer<TNode>   producer;
        private readonly ResourceAllocator allocator;
        private ILogging logging;
        private Msg priorInput;
        private bool isVerifier;

        public DeterministicParser(
            IProducer<TNode>      producer,
            RuntimeEbnfGrammar     grammar,
            TransitionDelegate    actionTable,
            ResourceAllocator     allocator,
            ILogging              logging
            )
            : this(
                producer,
                grammar,
                actionTable,
                allocator,
                logging,
                new TaggedStack<TNode>(InitialValueStackSize))
        {
            this.Reset();
        }

        private DeterministicParser(
            IProducer<TNode>      producer,
            RuntimeEbnfGrammar    grammar,
            TransitionDelegate    actionTable,
            ResourceAllocator     allocator,
            ILogging              logging,
            TaggedStack<TNode>    stateStack)
        {
            this.producer       = producer;
            this.grammar        = grammar;
            this.rules          = grammar.Productions.ToArray();
            this.actionTable    = actionTable;
            this.allocator      = allocator;
            this.logging        = logging;
            this.stateStack     = stateStack;
        }

        public void Reset()
        {
            stateStack.Clear();
            stateStack.Push(0, default(TNode));
        }

        public IReceiver<Msg> Done()
        {
            Loc location;
            HLoc hLocation;

            if (!object.Equals(priorInput, default(Msg)))
            {
                location = priorInput.Location.GetEndLocation();
                hLocation = priorInput.HLocation.GetEndLocation();
            }
            else
            {
                location = Loc.Unknown;
                hLocation = new HLoc(1, 1, 1, 1);
            }

            var eoi = new Msg(EbnfGrammar.Eoi, null, location, hLocation);
            return Next(eoi);
        }

        public IReceiver<Msg> Next(Msg envelope)
        {
            stateStack.BeginEdit();

            int id = envelope.Id;
            MsgData data = envelope.FirstData;

        START:
            ParserAction action = LookaheadAction(id);
            switch (action.Kind)
            {
                case ParserActionKind.Fail:
                    if (isVerifier)
                    {
                        return null;
                    }

                    // ReportUnexpectedToken(msg, stateStack.PeekTag());
                    return RecoverFromError(envelope);
                case ParserActionKind.Resolve:
                    id = action.RolvedToken;
                    while (true)
                    {
                        if (data.TokenId == id)
                        {
                            // Successfully resolved to a particular token 
                            goto START;
                        }

                        data = data.Next;
                        if (data == null)
                        {
                            // Desired token was not present in Msg
                            goto case ParserActionKind.Fail;
                        }
                    }
                case ParserActionKind.Fork:
                case ParserActionKind.Conflict:
                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Location = envelope.Location,
                            HLocation = envelope.HLocation,
                            Message = "Hit parser conflict on token " + grammar.SymbolName(envelope.Id)
                        });
                    return null;

                case ParserActionKind.Shift:
                    {
                        stateStack.Push(action.State, producer.CreateLeaf(envelope, data));
                        break;
                    }
                case ParserActionKind.ShiftReduce:
                    {
                        TNode value = producer.CreateLeaf(envelope, data);
                        do
                        {
                            stateStack.Push(-1, value);
                            this.currentRule = grammar.Productions[action.ProductionId];
                            stateStack.Start = stateStack.Count - currentRule.PatternTokens.Length;
                            value = producer.CreateBranch(
                                currentRule,
                                stateStack.PeekTail(currentRule.PatternTokens.Length),
                                (IStackLookback<TNode>)stateStack);
                            stateStack.Pop(currentRule.PatternTokens.Length);
                            action = ParserAction.Decode(actionTable(stateStack.PeekTag(), currentRule.OutcomeToken));
                        }
                        while (action.Kind == ParserActionKind.ShiftReduce);

                        if (action.Kind == ParserActionKind.Fail)
                        {
                            return null;
                        }

                        Debug.Assert(action.Kind == ParserActionKind.Shift);
                        stateStack.Push(action.State, value);
                        break;
                    }
                
                case ParserActionKind.Accept:
                    producer.Result = stateStack.Peek();
                    return FinalReceiver<Msg>.Instance;
                default:
                    throw new InvalidOperationException("Internal error: Unsupported parser action");
            }

            stateStack.EndEdit();
            this.priorInput = envelope;
            return this;
        }

        private IReceiver<Msg> RecoverFromError(Msg currentInput)
        {
            if (currentInput.Id == EbnfGrammar.Eoi)
            {
                if (!isVerifier)
                {
                    logging.Write(
                        new LogEntry
                        {
                            Severity  = Severity.Error,
                            Message   = "Unexpected end of file.",
                            Location  = currentInput.Location,
                            HLocation = currentInput.HLocation,
                        });
                }

                return null;
            }

            this.producer = producer.GetErrorRecoveryProducer();

            IReceiver<Msg> result = new LocalCorrectionErrorRecovery(grammar, this, logging);
            if (priorInput != null)
            {
                stateStack.Undo(1);
                result = result.Next(priorInput);
            }
            else
            {
                stateStack.Undo(0);
            }

            return result.Next(currentInput);
        }

        private void ReportUnexpectedToken(Msg msg, int state)
        {
            object token = msg.Value;

            var message = new StringBuilder();

            message.Append("Got ").Append(msg.Value ?? grammar.SymbolName(msg.Id));
            message.Append("  but expected ");

            int[] expectedTokens = GetExpectedTokens(state);
            if (expectedTokens.Length == 0)
            {
                throw new InvalidOperationException("Internal error: invalid parser state: ");
            }
            else
            {
                message.Append(" ").Append(string.Join(" or ", expectedTokens.Select(grammar.SymbolName)));
            }

            //message.Append("  State stack [" + string.Join(", ", stateStack.Data.Take(stateStack.Count).Select(st => st.Tag)) + "]");

            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Error,
                    Location = msg.Location,
                    HLocation = msg.HLocation,
                    Message  = message.ToString()
                });
        }

        private int[] GetExpectedTokens(int state)
        {
            var result = new List<int>();
            int tokenCount = grammar.SymbolCount;

            for (int i = 0; i != tokenCount; ++i)
            {
                var action = actionTable(state, i);
                if (action != ParserAction.FailActionCell && grammar.IsTerminal(i))
                {
                    result.Add(i);
                }
            }

            return result.ToArray();
        }

        public IPushParser CloneVerifier()
        {
            var result = new DeterministicParser<object>(
                NullProducer<object>.Instance,
                grammar,
                actionTable,
                allocator,
                NullLogging.Instance,
                stateStack.CloneWithoutData());

            result.isVerifier = true;

            return result;
        }

        public IReceiver<Msg> ForceNext(params Msg[] input)
        {
            isVerifier = true;
            try
            {
                while (this.CloneVerifier().Feed(input) == null)
                {
                    if (stateStack.Count == 1)
                    {
                        return null;
                    }

                    stateStack.Pop(1);
                }
            }
            finally
            {
                isVerifier = false;
            }

            this.Feed(input);

            return this;
        }

        private ParserAction LookaheadAction(int token)
        {
            var act = ParserAction.Decode(actionTable(stateStack.PeekTag(), token));
            TNode value;

            while (act.Kind == ParserActionKind.Reduce)
            {
                this.currentRule = grammar.Productions[act.ProductionId];
                stateStack.Start = stateStack.Count - currentRule.PatternTokens.Length;
                value = producer.CreateBranch(
                            currentRule,
                            stateStack.PeekTail(currentRule.PatternTokens.Length),
                            (IStackLookback<TNode>)stateStack);

                stateStack.Pop(currentRule.PatternTokens.Length);
                act = ParserAction.Decode(actionTable(stateStack.PeekTag(), currentRule.OutcomeToken));

                while (act.Kind == ParserActionKind.ShiftReduce) // == GotoReduce
                {
                    stateStack.Push(-1, value);

                    this.currentRule = grammar.Productions[act.ProductionId];
                    stateStack.Start = stateStack.Count - currentRule.PatternTokens.Length;
                    value = producer.CreateBranch(
                            currentRule,
                            stateStack.PeekTail(currentRule.PatternTokens.Length),
                            (IStackLookback<TNode>)stateStack);

                    stateStack.Pop(currentRule.PatternTokens.Length);
                    act = ParserAction.Decode(actionTable(stateStack.PeekTag(), currentRule.OutcomeToken));
                }

                stateStack.Push(act.State, value);

                // reduce or final shift
                act = ParserAction.Decode(actionTable(act.State, token));
            }

            return act;
        }
    }
}
