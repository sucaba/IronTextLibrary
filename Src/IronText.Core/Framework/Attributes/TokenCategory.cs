
namespace IronText.Framework
{
    public enum TokenCategory
    {
        None            = 0x00,

        ExplicitlyUsed  = 0x01,
        Beacon          = 0x02,
        DoNotInsert     = 0x04,
        DoNotDelete     = 0x08,
        External        = 0x10,
    }
}
