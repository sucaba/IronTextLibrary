
namespace IronText.Runtime
{

#if !ENABLE_SEM0
    public delegate void ReductionAction(object dataContext);
#else
    public delegate void ReductionAction(IReductionContext dataContext);
    public interface IReductionContext
    {
        object GetSynthesized(int synthIndex);
        void SetSynthesized(int synthIndex, object value);

        object GetSynthesized(int position, int synthIndex);

        object GetInherited(int inhIndex);
    }
#endif
}
