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

        GssBackLink<T> PushShift(
            GssNode<T> priorNode,
            int        toState,
            T          label);

        GssBackLink<T> PushReduced(
            GssNode<T>  priorNode,
            int         toState,
            T           label,
            int         lookahead);
    }
}