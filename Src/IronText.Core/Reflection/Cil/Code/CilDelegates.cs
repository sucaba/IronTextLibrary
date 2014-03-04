
namespace IronText.Reflection.Managed
{
    /// <summary>
    /// Parser action builder contract
    /// </summary>
    /// <remarks>
    /// Generated code should leave <see cref="Object"/> return value in the CLR stack.
    /// </remarks>
    public delegate void CilActionContextLoader(IProductionActionCode code);

    /// <summary>
    /// Parser action builder contract
    /// </summary>
    /// <remarks>
    /// Generated code should leave <see cref="Object"/> return value in the CLR stack.
    /// </remarks>
    public delegate void CilProductionActionBuilder(IProductionActionCode code);

    /// <summary>
    /// Scanner action builder contract
    /// </summary>
    /// <param name="context"></param>
    public delegate void CilScanActionBuilder(IMatcherActionCode context);

    /// <summary>
    /// Merge action builder contract
    /// </summary>
    /// <remarks>
    /// Generated code should leave <see cref="Object"/> return value in the CLR stack.
    /// </remarks>
    public delegate void CilMergerActionBuilder(IMergeActionCode code);
}
