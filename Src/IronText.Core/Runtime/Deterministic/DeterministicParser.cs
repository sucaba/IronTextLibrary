﻿using IronText.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IronText.Runtime
{
    class DeterministicParser<TNode> : IPushParser
    {
        private const int InitialValueStackSize = 32;

        private readonly RuntimeGrammar grammar;
        private readonly TransitionDelegate actionTable;

        private readonly TaggedStack<TNode> stateStack;

        private RuntimeProduction currentRule;
        private IProducer<TNode> producer;
        private ILogging logging;
        private Msg priorInput;
        private bool isVerifier;

        public DeterministicParser(
            IProducer<TNode>   producer,
            RuntimeGrammar     grammar,
            TransitionDelegate actionTable,
            ILogging           logging)
            : this(
                producer,
                grammar,
                actionTable,
                logging,
                new TaggedStack<TNode>(InitialValueStackSize))
        {
            this.Reset();
        }

        private DeterministicParser(
            IProducer<TNode>   producer,
            RuntimeGrammar     grammar,
            TransitionDelegate actionTable,
            ILogging           logging,
            TaggedStack<TNode> stateStack)
        {
            this.producer    = producer;
            this.grammar     = grammar;
            this.actionTable = actionTable;
            this.logging     = logging;
            this.stateStack  = stateStack;
        }

        public void Reset()
        {
            stateStack.Clear();
            PushNode(0, producer.CreateStart());
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

            var eoi = new Msg(PredefinedTokens.Eoi, null, null, location, hLocation);
            return Next(eoi);
        }

        public IReceiver<Msg> Next(Msg envelope)
        {
            stateStack.BeginEdit();

            int id = envelope.AmbToken;
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
                    id = action.ResolvedToken;
                    while (true)
                    {
                        if (data.Token == id)
                        {
                            // Successfully resolved to a particular token 
                            goto START;
                        }

                        data = data.NextAlternative;
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
                            Message = "Hit parser conflict on token " + grammar.SymbolName(envelope.AmbToken)
                        });
                    return null;

                case ParserActionKind.Shift:
                    {
                        var node = producer.CreateLeaf(envelope, data);
                        PushNode(action.State, node);
                        break;
                    }
                case ParserActionKind.ShiftReduce:
                    {
                        TNode value = producer.CreateLeaf(envelope, data);
                        do
                        {
                            PushNode(-1, value);
                            this.currentRule = grammar.Productions[action.ProductionId];
                            value = producer.CreateBranch(
                                currentRule,
                                stateStack.PeekTail(currentRule.InputLength),
                                stateStack.ShiftedLookback(currentRule.InputLength));
                            stateStack.Pop(currentRule.InputLength);
                            action = ParserAction.Decode(actionTable(stateStack.PeekTag(), currentRule.Outcome));
                        }
                        while (action.Kind == ParserActionKind.ShiftReduce);

                        if (action.Kind == ParserActionKind.Fail)
                        {
                            return null;
                        }

                        Debug.Assert(action.Kind == ParserActionKind.Shift);
                        PushNode(action.State, value);
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
            if (currentInput.AmbToken == PredefinedTokens.Eoi)
            {
                if (!isVerifier)
                {
                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Message = "Unexpected end of file.",
                            Location = currentInput.Location,
                            HLocation = currentInput.HLocation,
                        });
                }

                return null;
            }

            this.producer = producer.GetRecoveryProducer();

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
            var message = new StringBuilder();

            // TODO: Get rid of grammar usage. Properly formatted text should be sufficient.
            message.Append("Got ").Append(msg.Text ?? grammar.SymbolName(msg.AmbToken));
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
                    Message = message.ToString()
                });
        }

        private int[] GetExpectedTokens(int parserState)
        {
            var result = new List<int>();
            int tokenCount = grammar.TokenCount;

            for (int i = 0; i != tokenCount; ++i)
            {
                var action = actionTable(parserState, i);
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
                value = producer.CreateBranch(
                            currentRule,
                            stateStack.PeekTail(currentRule.InputLength),
                            stateStack.ShiftedLookback(currentRule.InputLength));

                stateStack.Pop(currentRule.InputLength);
                act = ParserAction.Decode(actionTable(stateStack.PeekTag(), currentRule.Outcome));

                while (act.Kind == ParserActionKind.ShiftReduce) // == GotoReduce
                {
                    PushNode(-1, value);

                    this.currentRule = grammar.Productions[act.ProductionId];
                    value = producer.CreateBranch(
                            currentRule,
                            stateStack.PeekTail(currentRule.InputLength),
                            stateStack.ShiftedLookback(currentRule.InputLength));

                    stateStack.Pop(currentRule.InputLength);
                    act = ParserAction.Decode(actionTable(stateStack.PeekTag(), currentRule.Outcome));
                }

                PushNode(act.State, value);

                // reduce or final shift
                act = ParserAction.Decode(actionTable(act.State, token));
            }

            return act;
        }

        private void PushNode(int state, TNode value)
        {
            stateStack.Push(state, value);

            if (state >= 0)
            {
                producer.Shifted(stateStack);
            }
        }
    }
}
