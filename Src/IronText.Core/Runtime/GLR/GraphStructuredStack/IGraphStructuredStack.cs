using IronText.Diagnostics;

namespace IronText.Runtime
{
    delegate T ValueMergeDelegate<T>(T currentValue, T newValue);

    interface IGraphStructuredStack<T>
        : IUndoable
    {
        bool IsFrontEmpty { get; }
        ImmutableArray<GssNode<T>> Front { get; }
        GssNode<T> GetFrontNode(int state, int lookahead);

        Gss<T> CloneWithoutData();

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