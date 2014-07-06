namespace IronText.Collections
{
    public interface IDuplicateResolver<T>
    {
        T Resolve(T existingItem, T newItem);
    }
}