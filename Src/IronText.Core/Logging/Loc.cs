using System.Collections.Generic;

namespace IronText.Logging
{
    /// <summary>
    /// Represents location in text source.
    /// </summary>
    public struct Loc
    {
        public const string MemoryString = "<string>";
        public const string UnknownFile = "<unknown>";

        public const int UnknownPosition = -1;

        public const int DefaultEnd = -1;

        /// <summary>
        /// Unknown location
        /// </summary>
        public readonly static Loc Unknown = new Loc(UnknownFile, UnknownPosition);

        /// <summary>
        /// Url or Path to the document file
        /// </summary>
        public readonly string FilePath;

        /// <summary>
        /// Zero based start character index in document.
        /// </summary>
        public readonly int    Position;

        /// <summary>
        /// Length of the location in characters
        /// </summary>
        public readonly int    End;

        public int Length { get { return End - Position; } }

        /// <summary>
        /// Determines whether location is unknown.
        /// </summary>
        public bool IsUnknown { get { return Position < 0 || FilePath == null; } }

        /// <summary>
        /// Creates instance of the type <see cref="Loc"/> from the file path and character position
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="position">Zero-based index of character in file.</param>
        /// <param name="end">Length of the location in characters.</param>
        /// <returns></returns>
        public Loc(string filePath, int position, int end = DefaultEnd)
        {
            this.FilePath = filePath;
            this.Position = position;

            if (end == DefaultEnd)
            {
                this.End = position;
            }
            else
            {
                this.End = end;
            }
        }

        public Loc(int position, int end = DefaultEnd)
            : this(Loc.MemoryString, position, end) 
        { }

        #region Object overrides

        public override bool Equals(object obj) { return this == (Loc)obj; }

        public override int GetHashCode()
        {
            unchecked { return (FilePath ?? "").GetHashCode() + Position + Length; }
        }

        public override string ToString()
        {
            if (Length == DefaultEnd)
            {
                return string.Format("<Loc file={0}, pos={1}>", FilePath, Position);
            }
            else
            {
                return string.Format("<Loc file={0}, pos={1}, len={2}>", FilePath, Position, Length);
            }
        }

        #endregion

        public static bool operator==(Loc x, Loc y)
        {
            return x.Position == y.Position && x.Length == y.Length && x.FilePath == y.FilePath;
        }

        public static bool operator !=(Loc x, Loc y) { return !(x == y); }

        /// <summary>
        /// Glue of source locations
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Loc operator+(Loc x, Loc y)
        {
            if (x.Position < 0 || x.FilePath != y.FilePath)
            {
                return y;
            }

            int begin = x.Position < y.Position ?  x.Position : y.Position;
            int end   = x.End > y.End ? x.End : y.End;
            return new Loc(x.FilePath, begin, end);
        }

        public Loc GetStartLocation()
        {
            return new Loc(FilePath, Position, Position);
        }

        public Loc GetEndLocation()
        {
            return new Loc(FilePath, End, End);
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
    }
}
