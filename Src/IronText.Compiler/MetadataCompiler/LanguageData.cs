using System;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;
using System.Collections.ObjectModel;
using IronText.Extensibility;
using System.Linq;
using IronText.Automata.Regular;
using IronText.Automata.Lalr1;
using IronText.Reflection;
using IronText.Compiler;
using IronText.Compiler.Analysis;
using IronText.Reporting;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    internal class LanguageData
    {
        public Grammar                  Grammar { get; set; }

        public GrammarAnalysis          GrammarAnalysis { get; set; }

        public bool                     IsDeterministic;
        public Type                     DefinitionType;
        public DotState[]               ParserStates;

        public ProductionContextLink[]  LocalParseContexts;
        public ITable<int>              ParserActionTable;
        public int[]                    ParserConflictActionTable;
        public int[]                    StateToSymbolTable;
    }
}
