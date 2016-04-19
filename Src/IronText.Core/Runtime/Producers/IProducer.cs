using IronText.Algorithm;

namespace IronText.Runtime
{
    /// <summary>
    /// Represents abstract producer for parser results.
    /// </summary>
    /// <remarks>
    /// This absraction can represent following scenarios:
    ///  - Grammar actions. Values are merged according to a grammar merge rules (like in Elkhound).
    ///  - SPPF (Shared Packed Parse Forest). SPPF is bulit as it is described in paper 
    ///    (Right-nullable GLR parsers).
    ///  - AST. Classical AST is built when there is single derivation alternative, otherwise
    ///    alternative derivation branches are merged according to the user defined logic or failure is
    ///    is reported.
    /// </remarks>
    /// <typeparam name="T">Node of the tree or just merged value representation</typeparam>
    public interface IProducer<T>
    {
        ReductionOrder ReductionOrder { get; }

        T Result { get; set; }

        T CreateStart();

        // Leaf corresponding to the input terminal
        T CreateLeaf(Msg envelope, MsgData data);

        // Branch for production rule
        T CreateBranch(RuntimeProduction rule, ArraySlice<T> parts, IStackLookback<T> lookback);

        // Merge derivation alternatives
        T Merge(T alt1, T alt2, IStackLookback<T> lookback);

        // Get epsilon node corresponding to the non-term
        T GetDefault(int token, IStackLookback<T> lookback);

        // Producer used just before error recovery start
        IProducer<T> GetRecoveryProducer();

        void Shifted(int topState, IStackLookback<T> lookback);
    }
}
