
using System.Collections.Generic;
namespace IronText.Framework
{
    public struct HLoc
    {
        public readonly static HLoc Unknown = default(HLoc);

        public readonly int FirstLine;
        public readonly int FirstColumn;
        public readonly int LastLine;
        public readonly int LastColumn;

        public HLoc(int firstLine, int firstColumn, int lastLine, int lastColumn)
        {
            FirstLine   = firstLine;
            FirstColumn = firstColumn;
            LastLine    = lastLine;
            LastColumn  = lastColumn;
        }

        public HLoc GetEndLocation()
        {
            return new HLoc(LastLine, LastColumn, LastLine, LastColumn);
        }

        public static HLoc operator +(HLoc x, HLoc y)
        {
            if (x.IsUnknown)
            {
                return y;
            }

            if (y.IsUnknown)
            {
                return y;
            }

            return new HLoc(x.FirstLine, x.FirstColumn, y.LastLine, y.LastColumn);
        }

        public bool IsUnknown
        {
            get { return FirstLine == 0 || FirstColumn == 0; }
        }

        public static HLoc Sum(IEnumerable<HLoc> locations)
        {
            HLoc result = HLoc.Unknown;
            foreach (var loc in locations)
            {
                result = result + loc;
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format(
                "{0},{1} - {2},{3}",
                FirstLine,
                FirstColumn,
                LastLine,
                LastColumn);
        }
    }
}
