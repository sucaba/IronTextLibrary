#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class EncodedLrParserTable : ILrParserTable<int>
    {
        private readonly int[] conflictActionTable;

        public EncodedLrParserTable(ILrParserTable<ParserAction> original)
        {
            this.conflictActionTable = Array.ConvertAll(
                                        original.GetConflictActionTable(),
                                        ParserAction.Encode);
        }

        public ParserConflictInfo[] Conflicts { get; }

        public bool RequiresGlr { get; 
        }

        public int[] GetConflictActionTable()
        {
        }

        public ITable<int> GetParserActionTable()
        {
        }
    }
}
#endif
