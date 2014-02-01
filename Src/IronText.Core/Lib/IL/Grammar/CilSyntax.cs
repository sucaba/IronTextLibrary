using IronText.Framework;
using IronText.Lib.Ctem;

namespace IronText.Lib.IL
{
    // Namespace tags
    public interface ResolutionScopes { }
    public interface Types   { }
    public interface Methods { }
    public interface Args    { }
    public interface Locals  { }
    
    [Vocabulary]
    [StaticContext(typeof(Builtins))]
    [StaticContext(typeof(CilPrimitives))]
    [UseToken(typeof(QStr))]
    public interface CilSyntax
    {
        [LanguageService]
        IParsing Parsing { get; set; }

        [SubContext]
        CtemScanner Scanner { get; }

        [SubContext]
        IResolutionScopeNs ResolutionScopeNs { get; }

        [SubContext]
        ITypeNs Types { get; }

        [SubContext]
        IMethodNs Methods { get; }

        [Produce]
        CilDocumentSyntax BeginDocument();
    }
}
