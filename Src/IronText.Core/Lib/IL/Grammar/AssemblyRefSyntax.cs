using IronText.Framework;
using IronText.Lib.Ctem;

namespace IronText.Lib.IL
{
    [Demand]
    public interface AssemblyRefSyntax
        : CustomAttributesSyntax<AssemblyRefSyntax>
    {
        [Produce(".hash", "=")]
        AssemblyRefSyntax Hash(Bytes hashBytes);

        [Produce(".publickeytoken", "=")]
        AssemblyRefSyntax PublicKeyToken(Bytes keyBytes);

        [Produce(".ver", null, ":", null, ":", null, ":", null)]
        AssemblyRefSyntax Version(int major, int minor, int build, int revision);

        [Produce(".locale")]
        AssemblyRefSyntax Locale(QStr qstr);

        [Produce(".locale", "=")]
        AssemblyRefSyntax Locale(Bytes localeBytes);

        [Produce("}")]
        CilDocumentSyntax EndAssemblyExtern();
    }
}
