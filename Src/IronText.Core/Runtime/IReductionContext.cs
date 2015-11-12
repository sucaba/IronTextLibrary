
namespace IronText.Runtime
{
    public delegate void ReductionAction(IReductionContext dataContext);

    public interface IReductionContext
    {
        object GetSynthesized(string name);
        void SetSynthesized(string name, object value);

        object GetSynthesized(int position, string name);

        object GetInherited(string name);
    }
}
