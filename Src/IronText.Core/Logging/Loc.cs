using System.Collections.Generic;
using static System.Math;

namespace IronText.Logging
{
    public struct Loc
    {
        public const string MemoryString = "<string>";
        public const string UnknownFile = "<unknown>";

        public readonly static Loc Unknown = default(Loc);

        public static Loc FromPos(int position, int count)
        {
            return FromPos(UnknownFile, position, count);
        }

        public static Loc FromPos(string filePath, int position, int count)
        {
            return new Loc(filePath, 1, position + 1, 1, position + Max(count, 1));
        }

        public readonly string FilePath;
        public readonly int FirstLine;
        public readonly int FirstColumn;
        public readonly int LastLine;
        public readonly int LastColumn;

        public Loc(int firstColumn, int lastColumn)
            : this(1, firstColumn, 1, lastColumn)
        {
        }

        public Loc(string filePath, int firstColumn, int lastColumn)
            : this(filePath, 1, firstColumn, 1, lastColumn)
        {
        }

        public Loc(int firstLine, int firstColumn, int lastLine, int lastColumn)
            : this(UnknownFile, firstLine, firstColumn, lastLine, lastColumn)
        {
        }

        public Loc(string filePath, int firstLine, int firstColumn, int lastLine, int lastColumn)
        {
            FilePath    = filePath;
            FirstLine   = firstLine;
            FirstColumn = firstColumn;
            LastLine    = lastLine;
            LastColumn  = lastColumn;
        }

        public Loc GetEndLocation()
        {
            return new Loc(LastLine, LastColumn, LastLine, LastColumn);
        }

        public static Loc operator +(Loc x, Loc y)
        {
            if (x.IsUnknown)
            {
                return y;
            }

            if (y.IsUnknown)
            {
                return x;
            }

            return new Loc(x.FirstLine, x.FirstColumn, y.LastLine, y.LastColumn);
        }

        public static bool operator ==(Loc x, Loc y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Loc x, Loc y)
        {
            return !(x == y);
        }

        public bool IsUnknown
        {
            get { return FirstLine == 0 || FirstColumn == 0; }
        }

        public static Loc Sum(IEnumerable<Loc> locations)
        {
            Loc result = Loc.Unknown;
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
