using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Compiler.Analysis;
using IronText.Extensibility;
using IronText.Reflection;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    internal class LanguageData
    {
        public Grammar      Grammar             { get; set; }

        public bool         IsDeterministic     { get; set; }

        public ITable<int>  ParserActionTable   { get; set; }

        public int[]        ParserConflictActionTable { get; set; }

        public int[]        StateToToken        { get; set; }

        public int[]        TokenComplexity     { get; set; }

        public LocalContextBinding[]  LocalParseContexts  { get; set; }
    }
}