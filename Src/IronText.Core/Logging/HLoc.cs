using System.Collections.Generic;
using static System.Math;

namespace IronText.Logging
{
    public struct HLoc
    {
        public const string MemoryString = "<string>";
        public const string UnknownFile = "<unknown>";

        public readonly static HLoc Unknown = default(HLoc);

        public static HLoc FromPos(int position, int count)
        {
            return FromPos(UnknownFile, position, count);
        }

        public static HLoc FromPos(string filePath, int position, int count)
        {
            return new HLoc(filePath, 1, position + 1, 1, position + Max(count, 1));
        }

        public readonly string FilePath;
        public readonly int FirstLine;
        public readonly int FirstColumn;
        public readonly int LastLine;
        public readonly int LastColumn;

        public HLoc(int firstColumn, int lastColumn)
            : this(1, firstColumn, 1, lastColumn)
        {
        }

        public HLoc(string filePath, int firstColumn, int lastColumn)
            : this(filePath, 1, firstColumn, 1, lastColumn)
        {
        }

        public HLoc(int firstLine, int firstColumn, int lastLine, int lastColumn)
            : this(UnknownFile, firstLine, firstColumn, lastLine, lastColumn)
        {
        }

        public HLoc(string filePath, int firstLine, int firstColumn, int lastLine, int lastColumn)
        {
            FilePath    = filePath;
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

        public static bool operator ==(HLoc x, HLoc y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(HLoc x, HLoc y)
        {
            return !(x == y);
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
