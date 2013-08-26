using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface ParamAttrSyntax1<TNext> 
    {
        [ParseGet("[", "in", "]")] 
        TNext In { get; }

        [ParseGet("[", "out", "]")] 
        TNext Out { get; }

        [ParseGet("[", "opt", "]")] 
        TNext Opt { get; }
    }
}
