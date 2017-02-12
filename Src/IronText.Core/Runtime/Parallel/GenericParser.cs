using IronText.Collections;
using IronText.Common;
using IronText.Logging;
using IronText.Runtime.RIGLR.GraphStructuredStack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IronText.Runtime
{
    class GenericParser<T> : IPushParser
    {
        private readonly RiGss<T> stack = new RiGss<T>();
        private readonly TransitionDelegate actionTable;
        private readonly RuntimeGrammar grammar;
        private readonly Dictionary<ReductionNode<T>, int> N = new Dictionary<ReductionNode<T>, int>();
        private readonly ProcessNodeLookup<T> P = new ProcessNodeLookup<T>();
        private readonly IProducer<T> producer;
        private readonly ILogging logging;

        private Loc lastLocation = new Loc(1, 1, 1, 1);

        public GenericParser(
            IProducer<T>       producer,
            RuntimeGrammar     grammar,
            TransitionDelegate actionTable,
            ILogging           logging)
        {
            this.grammar     = grammar;
            this.producer    = producer;
            this.actionTable = actionTable;
            this.logging     = logging;
        }

        public IReceiver<Message> Next(Message message)
        {
            lastLocation = message.Location;
            if (message.IsAmbiguous)
            {
                throw new NotImplementedException();
            }

            MessageData alernateInput = message;

            var term = producer.CreateLeaf(message, alernateInput);

            foreach (var process in stack.Current)
            {
                int start = actionTable(process.State, alernateInput.Token);
                ParserInstruction instruction = grammar.Instructions[start];

                switch (instruction.Operation)
                {
                    case ParserOperation.Shift:
                        stack.Pending.Add(
                            new Process<T>(
                                instruction.State,
                                new ReductionNode<T>(alernateInput.Token, term, process.Pending),
                                process.CallStack));
                        break;
                    case ParserOperation.ReduceGoto:
                        {
                            var production = grammar.Productions[instruction.Production];
                            var value = producer.CreateBranch(production, process.Pending);
                            var bottom = process.Pending.GetAtDepth(production.InputLength);
                            int nextState = instruction.Argument2;
                            stack.Current.Add(
                                new Process<T>(
                                    nextState,
                                    new ReductionNode<T>(instruction.Production, value, bottom),
                                    process.CallStack));
                        }
                        break;
                    case ParserOperation.Pop:
                        {
                            P.RegisterPop(process.CallStack, process.Pending);
                            stack.Current.AddRange(process.ImmutablePop());
                        }
                        break;
                    case ParserOperation.PushGoto:
                        {
                            int pushState = instruction.PushState;
                            int nextState = instruction.State;

                            ProcessNode<T> pushNode;
                            List<ReductionNode<T>> poppedPending;

                            if (P.TryGetPopped(pushState, out pushNode, out poppedPending))
                            {
                                ProcessBackLink<T> newBackLink = pushNode.LinkPrior(
                                                                    process.CallStack,
                                                                    process.Pending);
                                if (newBackLink != null)
                                {
                                    // Fork Re-popped along the new link
                                    foreach (var pop in poppedPending)
                                    {
                                        // Schedule pop again with different top-part of pending
                                        stack.Current.Add(
                                            new Process<T>(
                                                pushNode.State,
                                                process.Pending.ImmutableAppend(pop),
                                                newBackLink.Prior));
                                    }
                                }
                            }
                            else
                            {
                                pushNode = new ProcessNode<T>(
                                            pushState,
                                            process.CallStack,
                                            process.Pending);

                                P.RegisterPush(pushNode);

                                stack.Current.Add(
                                    new Process<T>(
                                        nextState,
                                        ReductionNode<T>.Null,
                                        pushNode));
                            }
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Invalid operation ${instruction.Operation}");
                }
            }

            stack.Next();

            return this;
        }

        public IPushParser CloneVerifier()
        {
            throw new NotImplementedException();
        }

        public IReceiver<Message> Done()
        {
            var eoi = new Message(PredefinedTokens.Eoi, null, null, lastLocation);
            return Next(eoi);
        }

        public IReceiver<Message> ForceNext(params Message[] msg)
        {
            throw new NotImplementedException();
        }
    }
}
