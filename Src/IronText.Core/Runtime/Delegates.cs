
namespace IronText.Runtime
{
    public delegate int TransitionDelegate(int state, int token);

    public delegate int Scan1Delegate(ScanCursor cursor);

    public delegate object TermFactoryDelegate(object context, int action, string text);

    public delegate object ProductionActionDelegate(
        int         ruleId,      // rule being reduced
        StackNode[] parts,       // array containing path being reduced
        int         firstIndex,  // starting index of the path being reduced
        object      context,     // user provided context
        IStackLookback<StackNode> lookback    // access to the prior stack states and values
        );

    public delegate object MergeDelegate(
        int     token,
        object  oldValue,
        object  newValue,
        object  context,        // user provided context
        IStackLookback<StackNode> lookback   // access to the prior stack states and values
        );
}
