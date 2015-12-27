
namespace IronText.Runtime
{
    public delegate void ReductionAction(object dataContext);

#if ENABLE_SEM0
    public delegate void ReductionAction(IReductionContext dataContext);
    public interface IReductionContext
    {
        object GetSynthesized(string name);
        void SetSynthesized(string name, object value);

        object GetSynthesized(int position, string name);

        object GetInherited(string name);
    }
#endif
}
