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

        [Produce("}")]
        CilDocumentSyntax EndClass();

        [Produce(".method")]
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
        [Produce("(")]
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
        [Produce("{")]
        EmitSyntax BeginBody();
    }
}
