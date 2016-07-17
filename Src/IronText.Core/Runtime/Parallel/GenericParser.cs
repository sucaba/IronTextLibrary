using IronText.Collections;
using IronText.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IronText.Runtime
{
    class GenericParser<TNode> : IPushParser
    {
        private readonly RuntimeGrammar      grammar;
        private readonly TransitionDelegate  actionTable;
        private readonly GenericStack<TNode> stack;

        private RuntimeProduction currentProd;
        private IProducer<TNode> producer;
        private ILogging logging;
        private Message priorInput;
        private bool isVerifier;
        private IReceiver<Message> next;

        public GenericParser(
            IProducer<TNode>   producer,
            RuntimeGrammar     grammar,
            TransitionDelegate actionTable,
            ILogging           logging)
            : this(
                producer,
                grammar,
                actionTable,
                logging,
                new GenericStack<TNode>(grammar.MaxParserThreadCount))
        {
            this.Reset();
        }

        private GenericParser(
            IProducer<TNode>   producer,
            RuntimeGrammar     grammar,
            TransitionDelegate actionTable,
            ILogging           logging,
            GenericStack<TNode> stack)
        {
            this.producer    = producer;
            this.grammar     = grammar;
            this.actionTable = actionTable;
            this.logging     = logging;
            this.stack       = stack;
            this.next        = this;
        }

        public void Reset()
        {
            next = this;
            stack.Clear();
            stack.AddThread(0, producer.CreateStart());
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
            stack.BeginEdit();

            try
            {
                ProcessTerm(envelope, envelope.Data);
            }
            finally
            {
                stack.EndEdit();
                priorInput = envelope;
            }

            return next;
        }

        private void ProcessTerm(Message envelope, MessageData data)
        {
            foreach (var thread in stack.Front)
            {
                if (stack.IsAlive(thread))
                {
                    ProcessTermAtState(envelope, data, thread);
                }
            }
        }

        private void ProcessTermAtState(
            Message envelope,
            MessageData data,
            ParserThread<TNode> thread)
        {
            int pos = actionTable(thread.State, envelope.AmbiguousToken);
            ProcessTerm(envelope, data, thread, pos);
        }

        private void ProcessTerm(
            Message             envelope,
            MessageData         data,
            ParserThread<TNode> thread,
            int                 start)
        {
            int pos = start;

            int id = envelope.AmbiguousToken;

            while (true)
            {
                var action = grammar.Instructions[pos];

                switch (action.Kind)
                {
                    case ParserActionKind.Restart:
                        pos = actionTable(thread.State, id);
                        continue;

                    case ParserActionKind.Exit:
                        return;

                    case ParserActionKind.Reduce:
                        {
                            int initialState = thread.State;
                            var value = ReduceNoPush(ref thread, ref action);
                            /*
                            if (initialState == action.State)
                            {
                                throw new InvalidOperationException("TODO: Parser recursion");
                            }
                            */

                            PushNode(ref thread, action.State, value);
                            break;
                        }

                    case ParserActionKind.Fail:
                        Die(thread, envelope.Location);
                        return;

                    case ParserActionKind.Resolve:
                        id = action.ResolvedToken;
                        data = data.Alternatives()
                                   .ResolveFirst(x => x.Token == id);

                        if (data == null)
                        {
                            // Desired token was not present in message
                            goto case ParserActionKind.Fail;
                        }

                        break;

                    case ParserActionKind.Fork:
                        Fork(thread, envelope, data, action.Value1);
                        break;

                    case ParserActionKind.Conflict:
                        throw new InvalidOperationException("Parser conflict error.");

                    case ParserActionKind.Shift:
                        {
                            var node = producer.CreateLeaf(envelope, data);
                            PushNode(ref thread, action.State, node);
                            break;
                        }

                    case ParserActionKind.Accept:
                        producer.Result = thread.Value;
                        next = FinalReceiver<Message>.Instance;
                        return;
                    default:
                        throw new InvalidOperationException("Internal error: Unsupported parser action");
                }

                ++pos;
            }
        }

        private void Die(ParserThread<TNode> thread, Loc location)
        {
            stack.Remove(thread);
            if (stack.IsEmpty)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = "Invalid syntax",
                        Location = location
                    });
                next = null;
            }
        }

        private void Fork(
            ParserThread<TNode> thread,
            Message             envelope,
            MessageData         data,
            int                 start)
        {
            var newThread = stack.Fork(thread);
            ProcessTerm(
                envelope,
                data,
                newThread,
                start);
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
                stack.Undo(1);
                result = result.Next(priorInput);
            }
            else
            {
                stack.Undo(0);
            }

            return result.Next(currentInput);
        }

        private void ReportUnexpectedToken(Message msg, int state)
        {
            var message = new StringBuilder();

            // TODO: Get rid of grammar usage. Properly formatted text should be sufficient.
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
            var result = new GenericParser<object>(
                NullProducer<object>.Instance,
                grammar,
                actionTable,
                NullLogging.Instance,
                stack.CloneWithoutData());

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
                    if (stack.HasLayers == 1)
                    {
                        return null;
                    }

                    stack.PopLayer();
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

        private TNode ReduceNoPush(ref ParserThread<TNode> thread, ref ParserAction action)
        {
            this.currentProd = grammar.Productions[action.ProductionId];

            var result = producer.CreateBranch(currentProd, thread);

            var newThread = stack.Pop(thread, currentProd.InputLength);
            var newAction = GetAction(newThread.State, currentProd.Outcome);
            if (newAction.Kind == ParserActionKind.Fail)
            {
                throw new InvalidOperationException("Internal parser error: reduce goto caused failure.");
            }

            thread = newThread;
            action = newAction;
            return result;
        }

        private void PushNode(ref ParserThread<TNode> thread, int state, TNode value)
        {
            thread = stack.Push(thread, state, value);

            if (state >= 0)
            {
                producer.Shifted(state, thread);
            }
        }
    }
}
