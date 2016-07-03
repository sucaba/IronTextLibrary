using IronText.Diagnostics;

namespace IronText.Runtime
{
    delegate T ValueMergeDelegate<T>(T currentValue, T newValue);

    interface IGraphStructuredStack<T>
        : IUndoable
    {
        int Count { get; }
        bool IsEmpty { get; }
        int CurrentLayer { get; }
        GssNode<T>[] FrontArray { get; }
        Gss<T> CloneWithoutData();
        GssNode<T> GetFrontNode(int state, int lookahead = -1);

        void PushLayer();
        void PopLayer();

        GssLink<T> Push(
            GssNode<T> leftNode,
            int rightState,
            T label,
            int lookahead = -1,
            ValueMergeDelegate<T> merge = null);
    }
}