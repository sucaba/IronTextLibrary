
namespace IronText.Runtime
{
    public delegate int TransitionDelegate(int state, int token);

    public delegate int Scan1Delegate(ScanCursor cursor);

    public delegate object TermFactoryDelegate(object context, int action, string text);

    /// <summary>
    /// Called on each shift to update inherited attributes
    /// </summary>
    public delegate void ShiftActionDelegate(
        IStackLookback<ActionNode> lookback);

    /// <summary>
    /// Called on each reduce to create synthesized attribute values
    /// </summary>
    /// <param name="token"></param>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    /// <param name="context">user provided context</param>
    /// <param name="lookback">access to the prior stack states and values</param>
    /// <returns></returns>
    public delegate object MergeDelegate(
        int     token,
        object  oldValue,
        object  newValue,
        object  context,
        IStackLookback<ActionNode> lookback);

    public delegate object ProductionActionDelegate(ProductionActionArgs args);

    public class ProductionActionArgs
#if ENABLE_SEM0
        : IReductionContext
#endif
    {
        private readonly int partCount;
        private readonly ActionNode resultNode;

        public ProductionActionArgs(
            int productionIndex,
            int count,
            object context,
            IStackLookback<ActionNode> lookback,
            ActionNode resultNode)
        {
            this.ProductionIndex = productionIndex;
            this.partCount       = count;
            this.Context         = context;
            this.Lookback        = lookback;
            this.resultNode      = resultNode;
        }

        public int      ProductionIndex { get; private set; }        

        public object   Context         { get; private set; }

        public IStackLookback<ActionNode> Lookback { get; private set; }

        public ActionNode GetSyntaxArgByBackOffset(int backoffset)
        {
            return Lookback.GetNodeAt(backoffset);
        }

#if ENABLE_SEM0
        public object GetSynthesized(int synthIndex)
        {
            return resultNode.GetSynthesizedProperty(synthIndex);
        }

        public void SetSynthesized(int synthIndex, object value)
        {
            this.resultNode.SetSynthesizedProperty(synthIndex, value);
        }

        public object GetInherited(int inhIndex)
        {
            ActionNode priorNode = Lookback.GetNodeAt(1 + partCount);

            object result = priorNode.GetInheritedStateProperty(inhIndex);
            return result;
        }

        public object GetSynthesized(int backoffset, int synthIndex)
        {
            var node = GetSyntaxArgByBackOffset(backoffset);
            var result = node.GetSynthesizedProperty(synthIndex);
            return result;
        }
#endif
    }
}
