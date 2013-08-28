
namespace IronText.Framework
{
    /// <summary>
    /// Contract for allocating important memory resources and measuring statistics
    /// on such resources for particular language.
    /// </summary>
    interface IResourceAllocator
    {
        object[] AllocateRuleValuesBuffer();
    }
}
