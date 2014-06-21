namespace IronText.Reflection.Reporting
{
    public interface ISemanticBinding
    {
        Production  ProvidingProduction { get; }

        Production  ConsumingProduction { get; }

        SemanticRef Reference           { get; }
    }
}
