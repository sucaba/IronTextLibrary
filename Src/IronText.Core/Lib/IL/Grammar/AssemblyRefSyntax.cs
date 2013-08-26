using IronText.Framework;
using IronText.Lib.Ctem;

namespace IronText.Lib.IL
{
    [Demand]
    public interface AssemblyRefSyntax
        : CustomAttributesSyntax<AssemblyRefSyntax>
    {
        [Parse(".hash", "=")]
        AssemblyRefSyntax Hash(Bytes hashBytes);

        [Parse(".publickeytoken", "=")]
        AssemblyRefSyntax PublicKeyToken(Bytes keyBytes);

        [Parse(".ver", null, ":", null, ":", null, ":", null)]
        AssemblyRefSyntax Version(int major, int minor, int build, int revision);

        [Parse(".locale")]
        AssemblyRefSyntax Locale(QStr qstr);

        [Parse(".locale", "=")]
        AssemblyRefSyntax Locale(Bytes localeBytes);

        [Parse("}")]
        CilDocumentSyntax EndAssemblyExtern();
    }
}
