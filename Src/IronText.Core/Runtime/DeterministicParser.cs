using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Extensibility;

namespace IronText.Framework
{
    class DeterministicParser<TNode> : IPushParser
    {
        private const int InitialValueStackSize = 32;

        private readonly BnfGrammar             grammar;
        private readonly TransitionDelegate     actionTable;

        private readonly TaggedStack<TNode> stateStack;

#if SWITCH_FEATURE
        private Func<object,int,IReciever<Msg>,IReciever<Msg>> makeSwitch;
#endif
        private readonly BnfRule[] rules;
        private BnfRule            currentRule;
        private IProducer<TNode>   producer;
        private readonly ResourceAllocator allocator;
        private ILogging logging;
        private Msg priorInput;
        private bool isVerifier;

        public DeterministicParser(
            IProducer<TNode>      producer,
            BnfGrammar            grammar,
            TransitionDelegate    actionTable,
            ResourceAllocator     allocator
#if SWITCH_FEATURE
            , Func<object,int,IReciever<Msg>,IReciever<Msg>> makeSwitch
#endif
            , ILogging logging
            )
            : this(
                producer,
                grammar,
                actionTable,
                allocator
#if SWITCH_FEATURE
                , makeSwitch
#endif
                , logging
                , new TaggedStack<TNode>(InitialValueStackSize)
                )
        {
            this.Reset();
        }

        private DeterministicParser(
            IProducer<TNode>      producer,
            BnfGrammar            grammar,
            TransitionDelegate    actionTable,
            ResourceAllocator     allocator
#if SWITCH_FEATURE
            , Func<object,int,IReciever<Msg>,IReciever<Msg>> makeSwitch
#endif
            , ILogging logging
            , TaggedStack<TNode> stateStack 
            )
        {
            this.producer       = producer;
            this.grammar        = grammar;
            this.rules          = grammar.Rules.ToArray();
            this.actionTable    = actionTable;
            this.allocator      = allocator;
#if SWITCH_FEATURE
            this.makeSwitch     = makeSwitch;
#endif
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
            var eoi = new Msg { Id = BnfGrammar.Eoi };
            if (!object.Equals(priorInput, default(Msg)))
            {
                eoi.Location = priorInput.Location.GetEndLocation();
                eoi.HLocation = priorInput.HLocation.GetEndLocation();
            }

            return Next(eoi);
        }

        public IReceiver<Msg> Next(Msg msg)
        {
            stateStack.BeginEdit();

            ParserAction action = LookaheadAction(msg.Id);

            switch (action.Kind)
            {
                case ParserActionKind.Fail:
                    if (isVerifier)
                    {
                        return null;
                    }

                    // ReportUnexpectedToken(msg, stateStack.PeekTag());
                    return RecoverFromError(msg);
                case ParserActionKind.Conflict:
                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Location = msg.Location,
                            HLocation = msg.HLocation,
                            Message = "Hit parser conflict on token " + grammar.TokenName(msg.Id)
                        });
                    return null;

#if SWITCH_FEATURE
                case ParserActionKind.Switch:
                    {
                        var reciever = makeSwitch(Context, action.ExternalToken, this);
                        return reciever.Next(msg);
                    }
#endif
                case ParserActionKind.Shift:
                    {
                        stateStack.Push(action.State, producer.CreateLeaf(msg));
                        break;
                    }
                case ParserActionKind.ShiftReduce:
                    {
                        TNode value = producer.CreateLeaf(msg);
                        do
                        {
                            stateStack.Push(-1, value);
                            this.currentRule = grammar.Rules[action.Rule];
                            stateStack.Start = stateStack.Count - currentRule.Parts.Length;
                            value = producer.CreateBranch(
                                currentRule,
                                stateStack.PeekTail(currentRule.Parts.Length),
                                (IStackLookback<TNode>)stateStack);
                            stateStack.Pop(currentRule.Parts.Length);
                            action = ParserAction.Decode(actionTable(stateStack.PeekTag(), currentRule.Left));
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
            this.priorInput = msg;
            return this;
        }

        private IReceiver<Msg> RecoverFromError(Msg currentInput)
        {
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

            message.Append("Got ").Append(msg.Value ?? grammar.TokenName(msg.Id));
            message.Append("  but expected ");

            int[] expectedTokens = GetExpectedTokens(state);
            if (expectedTokens.Length == 0)
            {
                throw new InvalidOperationException("Internal error: invalid parser state: ");
            }
            else
            {
                message.Append(" ").Append(string.Join(" or ", expectedTokens.Select(grammar.TokenName)));
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
            int tokenCount = grammar.TokenCount;

            for (int i = 0; i != tokenCount; ++i)
            {
                var action = actionTable(state, i);
                if (action != ParserAction.FailActionCell && grammar.IsTerm(i))
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
                this.currentRule = grammar.Rules[act.Rule];
                stateStack.Start = stateStack.Count - currentRule.Parts.Length;
                value = producer.CreateBranch(
                            currentRule,
                            stateStack.PeekTail(currentRule.Parts.Length),
                            (IStackLookback<TNode>)stateStack);

                stateStack.Pop(currentRule.Parts.Length);
                act = ParserAction.Decode(actionTable(stateStack.PeekTag(), currentRule.Left));

                while (act.Kind == ParserActionKind.ShiftReduce) // == GotoReduce
                {
                    stateStack.Push(-1, value);

                    this.currentRule = grammar.Rules[act.Rule];
                    stateStack.Start = stateStack.Count - currentRule.Parts.Length;
                    value = producer.CreateBranch(
                            currentRule,
                            stateStack.PeekTail(currentRule.Parts.Length),
                            (IStackLookback<TNode>)stateStack);

                    stateStack.Pop(currentRule.Parts.Length);
                    act = ParserAction.Decode(actionTable(stateStack.PeekTag(), currentRule.Left));
                }

                stateStack.Push(act.State, value);

                // reduce or final shift
                act = ParserAction.Decode(actionTable(act.State, token));
            }

            return act;
        }
    }
}
