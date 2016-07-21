using IronText.Collections;
using IronText.Logging;
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

        private RuntimeProduction currentProd;
        private IProducer<TNode> producer;
        private ILogging logging;
        private Message priorInput;
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

        public IReceiver<Message> Done()
        {
            Loc location;

            if (!object.Equals(priorInput, default(Message)))
            {
                location = priorInput.Location.GetEndLocation();
            }
            else
            {
                location = new Loc(1, 1, 1, 1);
            }

            var eoi = new Message(PredefinedTokens.Eoi, null, null, location);
            return Next(eoi);
        }

        public IReceiver<Message> Next(Message envelope)
        {
            stateStack.BeginEdit();

            try
            {
                int id = envelope.AmbiguousToken;
                MessageData data = envelope.Data;

            RESTART:
                int start = actionTable(stateStack.PeekTag(), id);

                while (true)
                {
                    var action = grammar.Instructions[start];

                    switch (action.Kind)
                    {
                        case ParserActionKind.Restart:
                            goto RESTART;

                        case ParserActionKind.Exit:
                            return this;

                        case ParserActionKind.Reduce:
                            {
                                var value = ReduceNoPush(ref action);
                                PushNode(action.State, value);
                                break;
                            }

                        case ParserActionKind.Fail:
                            if (isVerifier)
                            {
                                return null;
                            }

                            // ReportUnexpectedToken(msg, stateStack.PeekTag());
                            return RecoverFromError(envelope);

                        case ParserActionKind.Resolve:
                            id = action.ResolvedToken;
                            data = data.Alternatives()
                                       .ResolveFirst(x => x.Token == id);

                            if (data == null)
                            {
                                // Desired token was not present in Msg
                                goto case ParserActionKind.Fail;
                            }

                            break;

                        case ParserActionKind.Fork:
                        case ParserActionKind.Conflict:
                            logging.Write(
                                new LogEntry
                                {
                                    Severity = Severity.Error,
                                    Location = envelope.Location,
                                    Message = "Hit parser conflict on token " + grammar.SymbolName(envelope.AmbiguousToken)
                                });
                            return null;

                        case ParserActionKind.Shift:
                            {
                                var node = producer.CreateLeaf(envelope, data);
                                PushNode(action.State, node);
                                break;
                            }

                        case ParserActionKind.Accept:
                            producer.Result = stateStack.Peek();
                            return FinalReceiver<Message>.Instance;

                        default:
                            throw new InvalidOperationException("Internal error: Unsupported parser action");
                    }

                    ++start;
                }
            }
            finally
            {
                stateStack.EndEdit();
                priorInput = envelope;
            }
        }

        private IReceiver<Message> RecoverFromError(Message currentInput)
        {
            if (currentInput.AmbiguousToken == PredefinedTokens.Eoi)
            {
                if (!isVerifier)
                {
                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Message = "Unexpected end of file.",
                            Location = currentInput.Location,
                        });
                }

                return null;
            }

            this.producer = producer.GetRecoveryProducer();

            IReceiver<Message> result = new LocalCorrectionErrorRecovery(grammar, this, logging);
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

        private void ReportUnexpectedToken(Message msg, int state)
        {
            var message = new StringBuilder();

            message.Append("Got ").Append(msg.Text ?? grammar.SymbolName(msg.AmbiguousToken));
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
                    Message = message.ToString()
                });
        }

        private int[] GetExpectedTokens(int parserState)
        {
            var result = new List<int>();
            int tokenCount = grammar.TokenCount;

            for (int i = 0; i != tokenCount; ++i)
            {
                var action = grammar.Instructions[actionTable(parserState, i)];
                if (action != ParserAction.FailAction && grammar.IsTerminal(i))
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

        public IReceiver<Message> ForceNext(params Message[] input)
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

        private ParserAction GetAction(int state, int token)
        {
            int start = actionTable(state, token);
            var result = grammar.Instructions[start];
            return result;
        }

        private TNode ReduceNoPush(ref ParserAction action)
        {
            this.currentProd = grammar.Productions[action.ProductionId];

            var result = producer.CreateBranch(currentProd, stateStack);

            stateStack.Pop(currentProd.InputLength);
            action = GetAction(currentProd.Outcome);

            return result;
        }

        private ParserAction GetAction(int token)
        {
            return GetAction(stateStack.PeekTag(), token);
        }

        private void PushNode(int state, TNode value)
        {
            stateStack.Push(state, value);

            if (state >= 0)
            {
                producer.Shifted(state, stateStack);
            }
        }
    }
}
