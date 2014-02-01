using IronText.Framework;
using IronText.Lib.Ctem;

namespace IronText.Lib
{
    [Vocabulary]
    public static class Numbers
    {
        [Produce]
        public static int Int32(Num num) { return int.Parse(num.Text); }
    }
}
