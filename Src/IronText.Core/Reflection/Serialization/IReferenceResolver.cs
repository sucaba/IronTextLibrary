
namespace IronText.Reflection
{
    internal interface IReferenceResolver<T, TRef>
    {
        T Find(TRef reference);

        T Create(TRef reference);

        T Resolve(TRef reference);
    }
}
