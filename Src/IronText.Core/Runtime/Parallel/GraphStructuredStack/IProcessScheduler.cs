using System.Collections.Generic;

namespace IronText.Runtime.RIGLR.GraphStructuredStack
{
    interface IProcessScheduler<T>
    {
        bool HasItemsToConsume { get; }
        IEnumerable<Process<T>> Consume();

        void Init(int state);
        void EnqueuePop(Process<T> current);
        void EnqueuePushGoto(Process<T> current, int pushState, int nextState);
        void EnqueueShift(Process<T> current, int toState, T label, int currentLayer);
        void EnqueueShift(CallStackNode<T> callStack, ProcessData<T> bottom, int toState, T label, int currentLayer);
    }
}