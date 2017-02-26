namespace IronText.Reporting
{
    public interface ISemanticBinding
    {
        string ProvidingProductionText { get; }

        string ConsumingProductionText { get; }

        string ReferenceName           { get; }
    }
}
