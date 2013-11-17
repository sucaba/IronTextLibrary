using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    /// <summary>
    /// Indexing logic and constants
    /// </summary>
    static class TableIndexing
    {
        public const int NoId = -1;

        public static int IdFromIndex(int index) { return index; }

        public static int IndexFromId(int id) { return id; }
    }
}
