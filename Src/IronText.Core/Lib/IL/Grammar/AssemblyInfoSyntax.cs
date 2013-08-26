using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface AssemblyInfoSyntax
        : CustomAttributesSyntax<AssemblyInfoSyntax>
    {
        [Parse("}")]
        CilDocumentSyntax EndAssembly();
    }
}
