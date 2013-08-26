
namespace IronText.Lib.Shared
{
    public interface IFrame<TNs>
    {
        Def<TNs> Get(string name);
        // TODO: Better to remove unnamed define for generating more readable code
        Def<TNs> Define();
        Def<TNs> Define(string name);
    }
}
