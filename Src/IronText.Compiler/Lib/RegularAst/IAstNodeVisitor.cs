
namespace IronText.Lib.RegularAst
{
    public interface IAstNodeVisitor<TResult>
    {
        TResult Visit(CharSetNode node);

        TResult Visit(ActionNode node);

        TResult Visit(CatNode node);

        TResult Visit(OrNode node);

        TResult Visit(RepeatNode node);
    }

    public interface IAstNodeVisitor<TResult,T>
    {
        TResult Visit(CharSetNode node, T value);

        TResult Visit(ActionNode node, T value);

        TResult Visit(CatNode node, T value);

        TResult Visit(OrNode node, T value);

        TResult Visit(RepeatNode node, T value);
    }
}
