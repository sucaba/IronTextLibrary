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

        [Parse(")")]
        WantImplAttr EndArgs();

        WantMoreArgs Argument(Ref<Types> type, Def<Args> name);
    }

    [Demand]
    public interface WantArgs
        : WantArgsBase
        , ParamAttrSyntax1<WantArgs>
    {
        [Parse]
        new WantMoreArgs Argument(Ref<Types> type, Def<Args> name);
    }

    [Demand]
    public interface WantMoreArgs
        : WantArgsBase
        , ParamAttrSyntax1<WantMoreArgs>
    {
        [Parse(",")]
        new WantMoreArgs Argument(Ref<Types> type, Def<Args> name);
    }
}
