
namespace IronText.Reflection
{
    public enum SymbolCategory
    {
        None            = 0x00,

        ExplicitlyUsed  = 0x01,
        Beacon          = 0x02,
        DoNotInsert     = 0x04,
        DoNotDelete     = 0x08,
    }

    public static class SymbolCategoryExtensions
    {
        public static bool Has(this SymbolCategory self, SymbolCategory flag)
        {
            return (self & flag) == flag;
        }
    }
}
