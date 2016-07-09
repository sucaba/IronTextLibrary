namespace IronText.Runtime
{
    /// <summary>
    /// GSS layer has nodes on 2 stages:
    /// - Nodes created by shifts at the end of token processing 
    /// - Nodes created by reduces at the beginning of token processing
    /// </summary>
    public enum GssStage
    {
        FinalShift    = 0,
        InitialReduce = 1,
    }
}
