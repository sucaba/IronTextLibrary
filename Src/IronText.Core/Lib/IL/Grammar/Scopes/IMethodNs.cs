using System.Reflection;
using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    [Vocabulary]
    public interface IMethodNs
    {
        Ref<Methods> Import(MethodBase method);

        [ParseGet]
        WantMethodSig StartSignature { get; }

        [Parse]
        Ref<Methods> Method(Pipe<IMethodNs,DoneMethodSig> code);
    }

    [Demand]
    public interface WantMethodSig
        : WantCallConvSig
    {
    }

    [Demand]
    public interface WantCallConvSig
        : WantCallConvThen<WantCallConvSig>
        , WantCallKindSig
    {
    }

    [Demand]
    public interface WantCallKindSig
        : WantCallKindThen<WantCallKindSig>
        , WantReturnTypeSig
    {
    }

    [Demand]
    public interface WantReturnTypeSig
        : WantReturnTypeThen<WantDeclTypeSig>
    {
    }

    [Demand]
    public interface WantDeclTypeSig
        : WantNameSig
    {
        [Parse(null, "::")]
        WantNameSig DecaringType(TypeSpec typeSpec);
    }

    [Demand]
    public interface WantNameSig
        : WantNameThen<WantOpenArgsSig>
    {
    }

    [Demand]
    public interface WantOpenArgsSig
    {
        [Parse("(")]
        WantArgsSig BeginArgs();
    }

    public interface DoneMethodSig { }

    [Demand]
    public interface WantArgsSigBase
    {
        [Parse(")")]
        DoneMethodSig EndArgs();

        WantMoreArgsSig Argument(Ref<Types> type, string argName);
    }

    [Demand]
    public interface WantArgsSig
        : WantArgsSigBase
        , ParamAttrSyntax1<WantArgsSig>
    {
        [Parse]
        new WantMoreArgsSig Argument(Ref<Types> type, string argName);
    }

    [Demand]
    public interface WantMoreArgsSig
        : WantArgsSigBase
        , ParamAttrSyntax1<WantMoreArgsSig>
    {
        [Parse(",")]
        new WantMoreArgsSig Argument(Ref<Types> type, string argName);
    }
}
