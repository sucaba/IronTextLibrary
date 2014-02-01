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

        [Produce(".assembly", null, "{")]
        AssemblyInfoSyntax Assembly(string assemblyName);

        [Produce(".assembly", "extern", null, "{")]
        AssemblyRefSyntax AssemblyExtern(Def<ResolutionScopes> assemblyScope);

        [Produce(".module")]
        CilDocumentSyntax Module(QStr fileName);

        [Produce(".class")]
        ClassAttrSyntax Class_();

        [Produce]
        void EndDocument();
    }

    [Demand]
    public interface ClassIdSyntax
    {
        [Produce(null, "{")]
        ClassExtendsSyntax Named(string className);
    }

    [Demand]
    public interface ClassExtendsSyntax : ClassImplementsSyntax
    {
        [Produce("extends")]
        ClassImplementsSyntax Extends(Ref<Types> baseClass);
    }

    [Demand]
    public interface ClassImplementsSyntax : ClassSyntax
    {
        [Produce("implements")]
        ClassImplementsSyntax Implements(Ref<Types> @interface);
    }
}
