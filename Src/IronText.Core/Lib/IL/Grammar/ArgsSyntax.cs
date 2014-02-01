using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantArgsBase
    {
        [SubContext]
        DefFirstNs<Args> Args { get; }

        [SubContext]
        ITypeNs Types { get; }

        [Produce(")")]
        WantImplAttr EndArgs();

        WantMoreArgs Argument(Ref<Types> type, Def<Args> name);
    }

    [Demand]
    public interface WantArgs
        : WantArgsBase
        , ParamAttrSyntax1<WantArgs>
    {
        [Produce]
        new WantMoreArgs Argument(Ref<Types> type, Def<Args> name);
    }

    [Demand]
    public interface WantMoreArgs
        : WantArgsBase
        , ParamAttrSyntax1<WantMoreArgs>
    {
        [Produce(",")]
        new WantMoreArgs Argument(Ref<Types> type, Def<Args> name);
    }
}
