using IronText.Framework;
using System;

namespace IronText.Lib.IL
{
    [Demand]
    public interface ClassSyntax
    {
        [SubContext]
        ITypeNs Types { get; }

        [Produce(".pack")]
        ClassSyntax Pack(int value);

        [Produce(".size")]
        ClassSyntax Size(int value);

        [SubContext]
        IMethodNs Methods { get; }

        [Produce("}")]
        CilDocumentSyntax EndClass();

        [Produce(".method")]
        WantMethAttr Method();
    }

    public static class SyntaxExtensions
    {
        public static WantImplAttr PrivateStaticMethod(this ClassSyntax syntax, string name, Type delegateType)
        {
            var signature = GetDelegateSignature(delegateType);
            WantArgsBase args = syntax.Method()
                    .Private.Static
                    .Returning(syntax.Types.Import(signature.ReturnType))
                    .Named(name)
                    .BeginArgs();
            
            foreach (var param in signature.GetParameters())
            {
                args = args.Argument(syntax.Types.Import(param.ParameterType),  args.Args.Generate(param.Name));
            }

            return args.EndArgs();
        }

        private static System.Reflection.MethodInfo GetDelegateSignature(Type delegateType)
        {
            var result = delegateType.GetMethod("Invoke");
            return result;
        }
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
