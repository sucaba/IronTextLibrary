using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    [Demand]
    public interface CilDocumentSyntax
    {
        [SubContext]
        IResolutionScopeNs ResolutionScopeNs { get; }

        [SubContext]
        ITypeNs Types { get; }

        [SubContext]
        IMethodNs Methods { get; }

        // TODO: Create related language for assembly modifications
        AssemblyInfoSyntax AssemblyRewrite(string fromFilePath);

        [Parse(".assembly", null, "{")]
        AssemblyInfoSyntax Assembly(string assemblyName);

        [Parse(".assembly", "extern", null, "{")]
        AssemblyRefSyntax AssemblyExtern(Def<ResolutionScopes> assemblyScope);

        [Parse(".module")]
        CilDocumentSyntax Module(QStr fileName);

        [Parse(".class")]
        ClassAttrSyntax Class_();

        [Parse]
        void EndDocument();
    }

    [Demand]
    public interface ClassIdSyntax
    {
        [Parse(null, "{")]
        ClassExtendsSyntax Named(string className);
    }

    [Demand]
    public interface ClassExtendsSyntax : ClassImplementsSyntax
    {
        [Parse("extends")]
        ClassImplementsSyntax Extends(Ref<Types> baseClass);
    }

    [Demand]
    public interface ClassImplementsSyntax : ClassSyntax
    {
        [Parse("implements")]
        ClassImplementsSyntax Implements(Ref<Types> @interface);
    }
}
