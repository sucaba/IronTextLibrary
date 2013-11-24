
namespace IronText.Framework
{
    public enum TokenCategory
    {
        None            = 0x00,

        ExplicitlyUsed  = 0x01,
        Beacon          = 0x02,
        DoNotInsert     = 0x04,
        DoNotDelete     = 0x08,
    }

    public static class TokenCategoryExtensions
    {
        public static bool Has(this TokenCategory self, TokenCategory flag)
        {
            return (self & flag) == flag;
        }
    }
}
