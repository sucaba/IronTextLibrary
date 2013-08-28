namespace IronText.Extensibility
{
    /// <summary>
    /// Parser action builder contract
    /// </summary>
    /// <remarks>
    /// Generated code should leave <see cref="Object"/> return value in the CLR stack.
    /// </remarks>
    public delegate void GrammarActionBuilder(IGrammarActionCode code);

    /// <summary>
    /// Merge action builder contract
    /// </summary>
    /// <remarks>
    /// Generated code should leave <see cref="Object"/> return value in the CLR stack.
    /// </remarks>
    public delegate void MergeActionBuilder(IMergeActionCode code);

    /// <summary>
    /// Scanner action builder contract
    /// </summary>
    /// <param name="context"></param>
    public delegate void ScanActionBuilder(IScanActionCode context);

    /// <summary>
    /// Switch token-factory method builder
    /// </summary>
    /// <param name="code"></param>
    public delegate void SwitchActionBuilder(ISwitchActionCode code);

    /// <summary>
    /// Custom reporting action
    /// </summary>
    /// <param name="data">Language data for building various reports</param>
    public delegate void ReportBuilder(IReportData data);
}
