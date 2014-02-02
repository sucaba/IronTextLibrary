using IronText.Diagnostics;

namespace IronText.Runtime
{
    interface IGss<T>
    {
        int CurrentLayer { get; }

        bool IsEmpty { get; }

        int Count { get; }

        GssNode<T>[] FrontArray { get; }

        GssNode<T> GetFrontNode(int state, int lookahead = -1);

        Gss<T> CloneWithoutData();

        void WriteGraph(IGraphView view, RuntimeGrammar grammar, int[] stateToSymbol);

        void PushLayer();

        // Note:
        //  This one is different from the Undo() in following:
        //      - Undo() recovers state of GSS precisely while PopLayer() does not 
        //        restore nodes (parsing threads) which were lost during the parse 
        //        process.
        //      - Undo() has limited depth. Typically depth=1 which undoes current
        //        term effects and prior term effects. In contrast, PopLayer()
        //        can be applied until Gss.CurrentLayer is 0.
        //      - Undo() resores GSS.Front to the state it was before the corresponding input,
        //        while PopLayer() restores front the the state of the corresponding input shift.
        //        Difference is that shift can be preceded by some reduce actions which
        //        which are triggered by the lookahead.
        void PopLayer();

        GssLink<T> Push(GssNode<T> frontNode, int state, T value, int lookahead = -1);
    }
}
