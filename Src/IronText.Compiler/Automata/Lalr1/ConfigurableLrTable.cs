using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class ConfigurableLrTable : ILrParserTable
    {
        private readonly IMutableTable<ParserAction> data;
        private readonly GrammarAnalysis grammar;
        private readonly ILrParserTable  underlyingTable;

        public ConfigurableLrTable(
            ILrDfa dfa,
            RuntimeOptions flags,
            GrammarAnalysis grammar)
        {
            this.grammar = grammar;

            this.data = new MutableTable<ParserAction>(dfa.States.Length, grammar.TotalSymbolCount);

            underlyingTable = new CanonicalLrDfaTable(dfa, this.data, grammar);
            Configure(flags, underlyingTable.TargetRuntime); 
        }

        public bool ComplyWithConfiguration { get; private set; }

        public ParserRuntime TargetRuntime { get; private set; }

        public ITable<ParserAction> GetParserActionTable() { return data; }

        public ParserAction[] GetConflictActionTable()
        {
            return underlyingTable.GetConflictActionTable();
        }

        public ParserConflictInfo[] Conflicts { get { return underlyingTable.Conflicts; } }

        private ParserRuntime GetRuntime(RuntimeOptions flags, ParserRuntime actualRuntime)
        {
            switch (flags & RuntimeOptions.ParserAlgorithmMask)
            {
                case RuntimeOptions.ForceGeneric:
                    return ParserRuntime.Generic;
                case RuntimeOptions.ForceNonDeterministic:
                    return ParserRuntime.Glr;
                case RuntimeOptions.AllowNonDeterministic:
                case RuntimeOptions.ForceDeterministic:
                    return actualRuntime;
                default:
                    throw new InvalidOperationException(
                        "Internal error: unsupported language flags: " + (int)flags);
            }
        }

        private void Configure(RuntimeOptions flags, ParserRuntime actualRuntime)
        {
            TargetRuntime = GetRuntime(flags, actualRuntime);
            if ((flags & RuntimeOptions.ParserAlgorithmMask) == RuntimeOptions.ForceDeterministic)
            {
                ComplyWithConfiguration = TargetRuntime == actualRuntime;
            }
            else
            {
                ComplyWithConfiguration = true;
            }
        }

        private void Configure(ILrDfa dfa, RuntimeOptions flags, ParserRuntime actualRuntime)
        {
            ComplyWithConfiguration = true;
            switch (flags & RuntimeOptions.ParserAlgorithmMask)
            {
                case RuntimeOptions.ForceGeneric:
                    TargetRuntime = ParserRuntime.Generic;
                    break;
                case RuntimeOptions.ForceNonDeterministic:
                    TargetRuntime = ParserRuntime.Glr;
                    break;
                case RuntimeOptions.AllowNonDeterministic:
                    TargetRuntime = actualRuntime;
                    break;
                case RuntimeOptions.ForceDeterministic:
                    TargetRuntime = actualRuntime;
                    ComplyWithConfiguration = TargetRuntime == ParserRuntime.Deterministic;
                    break;
                default:
                    throw new InvalidOperationException(
                        "Internal error: unsupported language flags: " + (int)flags);
            }
        }
    }
}
