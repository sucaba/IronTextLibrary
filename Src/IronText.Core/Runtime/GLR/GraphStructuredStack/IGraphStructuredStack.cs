using IronText.Collections;
using System;

namespace IronText.Runtime
{
    interface IGraphStructuredStack<T>
        : IUndoable
    {
        ImmutableArray<GssNode<T>> Front { get; }
        GssNode<T> GetFrontNode(int state, int lookahead);

        bool HasLayers { get; }

        void PushLayer();
        void PopLayer();

        GssBackLink<T> Push(
            GssNode<T>  fromNode,
            int         toState,
            T           label,
            int         lookahead = -1,
            Func<T,T,T> merge = null);
    }
}