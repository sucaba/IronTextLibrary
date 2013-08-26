using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface ClassSyntax
    {
        [SubContext]
        ITypeNs Types { get; }

        [SubContext]
        IMethodNs Methods { get; }

        [Parse("}")]
        CilDocumentSyntax EndClass();

        [Parse(".method")]
        WantMethAttr Method();
    }

    [Demand]
    public interface WantMethAttr 
        : WantMethAttrThen<WantMethAttr>
        , WantCallConv
    {
    }

    [Demand]
    public interface WantCallConv 
        : WantCallConvThen<WantCallConv>
        , WantCallKind
    {
    }

    [Demand]
    public interface WantCallKind
        : WantCallKindThen<WantCallKind>
        , WantReturnType
    {
    }

    [Demand]
    public interface WantReturnType
        : WantReturnTypeThen<WantName>
    {
    }

    [Demand]
    public interface WantName
        : WantNameThen<WantOpenArgs>
    {
    }

    [Demand]
    public interface WantOpenArgs
    {
        [Parse("(")]
        WantArgs BeginArgs();
    }

    [Demand]
    public interface WantImplAttr 
        : WantImplAttrThen<WantImplAttr>
        , WantMethodBody
    {
    }

    [Demand]
    public interface WantMethodBody
    {
        [Parse("{")]
        EmitSyntax BeginBody();
    }
}
