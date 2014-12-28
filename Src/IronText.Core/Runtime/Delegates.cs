
namespace IronText.Runtime
{
    public delegate int TransitionDelegate(int state, int token);

    public delegate int Scan1Delegate(ScanCursor cursor);

    public delegate object TermFactoryDelegate(object context, int action, string text);

    public class ProductionActionArgs
    {
        private readonly ActionNode[] parts;
        private readonly int firstIndex;
        private int partCount;

        public ProductionActionArgs(
            int          productionIndex,
            ActionNode[] parts,
            int          firstIndex,
            object       context,
            IStackLookback<ActionNode> lookback)
        {
            this.ProductionIndex = productionIndex;
            this.parts           = parts;
            this.firstIndex      = firstIndex;
            this.partCount       = parts.Length - firstIndex;
            this.Context         = context;
            this.Lookback        = lookback;
        }

        public int ProductionIndex { get; private set; }        

        public ActionNode GetSyntaxArg(int index) { return parts[firstIndex + index]; }

        public int SyntaxArgCount { get {  return partCount; } }

        public object Context { get; private set; }

        public IStackLookback<ActionNode> Lookback { get; private set; }
    }

    public delegate object ProductionActionDelegate(ProductionActionArgs args);
    /*
        int         ruleId,      // rule being reduced
        ActionNode[] parts,       // array containing path being reduced
        int         firstIndex,  // starting index of the path being reduced
        object      context,     // user provided context
        IStackLookback<ActionNode> lookback    // access to the prior stack states and values
        );
    */

    public delegate object MergeDelegate(
        int     token,
        object  oldValue,
        object  newValue,
        object  context,        // user provided context
        IStackLookback<ActionNode> lookback   // access to the prior stack states and values
        );
}
