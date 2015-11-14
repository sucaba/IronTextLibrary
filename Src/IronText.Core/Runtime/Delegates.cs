
namespace IronText.Runtime
{
    public delegate int TransitionDelegate(int state, int token);

    public delegate int Scan1Delegate(ScanCursor cursor);

    public delegate object TermFactoryDelegate(object context, int action, string text);

    public class ProductionActionArgs : IReductionContext
    {
        private readonly ActionNode[] parts;
        private readonly int firstIndex;
        private readonly int partCount;
        private int      _syntaxArgCount;
        private readonly ActionNode resultNode;

        public ProductionActionArgs(
            int          productionIndex,
            ActionNode[] parts,
            int          firstIndex,
            int          count,
            object       context,
            IStackLookback<ActionNode> lookback,
            ActionNode   resultNode)
        {
            this.ProductionIndex = productionIndex;
            this.parts           = parts;
            this.firstIndex      = firstIndex;
            this.partCount       = count;
            this._syntaxArgCount = parts.Length - firstIndex;
            this.Context         = context;
            this.Lookback        = lookback;
            this.resultNode      = resultNode;
        }

        public int      ProductionIndex { get; private set; }        

        public int      SyntaxArgCount  { get {  return _syntaxArgCount; } }

        public object   Context         { get; private set; }

        public IStackLookback<ActionNode> Lookback { get; private set; }

        public ActionNode GetSyntaxArg(int index) { return parts[firstIndex + index]; }

        public object GetSynthesized(string name)
        {
            return resultNode.GetSynthesizedProperty(name);
        }

        public void SetSynthesized(string name, object value)
        {
            this.resultNode.SetSynthesizedProperty(name, value);
        }

        public object GetInherited(string name)
        {
            ActionNode priorNode = Lookback.GetNodeAt(1);

            object result = priorNode.GetInheritedStateProperty(name);
            return result;
        }

        public void SetInherited(string name, object value)
        {
            resultNode.SetInheritedStateProperty(name, value);
        }

        public object GetSynthesized(int position, string name)
        {
            var node = GetSyntaxArg(position);
            var result = node.GetSynthesizedProperty(name);
            return result;
        }
    }

    public delegate object ProductionActionDelegate(ProductionActionArgs args);

    public delegate object MergeDelegate(
        int     token,
        object  oldValue,
        object  newValue,
        object  context,        // user provided context
        IStackLookback<ActionNode> lookback   // access to the prior stack states and values
        );

    /// <summary>
    /// Called on each shift to update inherited attributes
    /// </summary>
    /// <param name="lookback"></param>
    /// <returns></returns>
    public delegate void ShiftActionDelegate(
        IStackLookback<ActionNode> lookback);
}
