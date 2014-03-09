
namespace IronText.Reflection.Managed
{
    /// <summary>
    /// Parser action builder contract
    /// </summary>
    /// <remarks>
    /// Generated code should leave <see cref="Object"/> return value in the CLR stack.
    /// </remarks>
    public delegate void CilActionContextLoader(IProductionCode code);

    /// <summary>
    /// Parser action builder contract
    /// </summary>
    /// <remarks>
    /// Generated code should leave <see cref="Object"/> return value in the CLR stack.
    /// </remarks>
    public delegate void CilProductionActionBuilder(IProductionCode code);

    /// <summary>
    /// Scanner action builder contract
    /// </summary>
    /// <param name="context"></param>
    public delegate void CilScanActionBuilder(IMatcherCode context);

    /// <summary>
    /// Merge action builder contract
    /// </summary>
    /// <remarks>
    /// Generated code should leave <see cref="Object"/> return value in the CLR stack.
    /// </remarks>
    public delegate void CilMergerActionBuilder(IMergerCode code);
}
