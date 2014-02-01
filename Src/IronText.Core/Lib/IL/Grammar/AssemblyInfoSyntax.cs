using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface AssemblyInfoSyntax
        : CustomAttributesSyntax<AssemblyInfoSyntax>
    {
        [Produce("}")]
        CilDocumentSyntax EndAssembly();
    }
}
