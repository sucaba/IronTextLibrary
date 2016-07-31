﻿using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Compiler.Analysis;
using IronText.Extensibility;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;
using IronText.Runtime.Semantics;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Precompiled language data
    /// </summary>
    internal class LanguageData
    {
        public Grammar          Grammar             { get; set; }

        public RuntimeGrammar   RuntimeGrammar      { get; set; }

        public ParserRuntime    TargetParserRuntime { get; set; }

        public ITable<ParserDecision> ParserActionTable   { get; set; }

        public ITable<int>      ParserActionStartTable   { get; set; }

        public ParserConflictInfo[] ParserConflicts { get; set; }

        public int[]            StateToToken        { get; set; }

        public int[]            TokenComplexity     { get; set; }

        public StackSemanticBinding[]  SemanticBindings  { get; set; }

        public int[]            MatchActionToToken  { get; set; }

        public ITdfaData        ScannerTdfa { get; set; }
    }
}