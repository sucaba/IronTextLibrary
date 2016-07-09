using IronText.Algorithm;
using System;

namespace IronText.Runtime.Inlining
{
    class InlinedProducer<T> : IProducer<T>
    {
        private FixedSizeStack<T> stack;
        private readonly RuntimeGrammar grammar;
        private readonly IProducer<T> producer;

        public ReductionOrder ReductionOrder
        {
            get { return producer.ReductionOrder; }
        }

        public T Result
        {
            get { return producer.Result; }
            set { producer.Result = value; }
        } 

        public InlinedProducer(RuntimeGrammar grammar, IProducer<T> producer)
        {
            this.grammar  = grammar;
            this.producer = producer;
        }
        
        public T CreateBranch(
            RuntimeProduction production,
            IStackLookback<T> input)
        {
            InlinedAction[] plan = grammar.GetReducePlan(production.Index);
            int count = plan.Length;
            for (int i = 0; i != count; ++i)
            {
                var instruction = plan[i];
                switch (instruction.Op)
                {
                    case InlinedActionOp.RetDirect:
                        return producer.CreateBranch(production, input);
                    case InlinedActionOp.Ret:
                        return stack.Peek();
                    case InlinedActionOp.Init:
                        this.stack = new FixedSizeStack<T>(instruction.StackSize);
                        break;
                    case InlinedActionOp.Read:
                        stack.Push(input.GetNodeAt(instruction.BackOffset));
                        break;
                    case InlinedActionOp.Reduce:
                        var inlined = grammar.Productions[instruction.ProductionIndex];
                        var outcome = producer.CreateBranch(
                                        inlined,
                                        stack.GetLookback());

                        stack.Pop(inlined.InputLength);
                        stack.Push(outcome);
                        break;
                    case InlinedActionOp.Shifted:
                        producer.Shifted(instruction.ShiftedState, stack.GetLookback());
                        break;
                    default:
                        throw new InvalidOperationException($"Not supported instruction {instruction.Op}");
                }
            }

            throw new InvalidOperationException($"Invalid plan for production #{production.Index}");
        }

        public T CreateStart()
        {
            return producer.CreateStart();
        }

        public T CreateLeaf(Message envelope, MessageData data)
        {
            return producer.CreateLeaf(envelope, data);
        }

        public T Merge(T alt1, T alt2, IStackLookback<T> lookback)
        {
            return producer.Merge(alt1, alt2, lookback);
        }

        public IProducer<T> GetRecoveryProducer()
        {
            return new InlinedProducer<T>(
                    grammar,
                    producer.GetRecoveryProducer());
        }

        public void Shifted(int topState, IStackLookback<T> lookback)
        {
            if (!grammar.IsInlinedState(topState))
            {
                producer.Shifted(topState, lookback);
            }
        }
    }
}
