using IronText.Diagnostics;
using System;

namespace IronText.Runtime
{
    interface IGraphStructuredStack<T>
        : IUndoable
    {
        ImmutableArray<GssNode<T>> Front { get; }
        GssNode<T> GetFrontNode(int state, int lookahead);

        void PushLayer();
        void PopLayer();

        GssLink<T> Push(
            GssNode<T> leftNode,
            int rightState,
            T label,
            int lookahead = -1,
            Func<T,T,T> merge = null);
    }
}