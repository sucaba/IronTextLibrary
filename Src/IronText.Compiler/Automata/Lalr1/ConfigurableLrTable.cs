using System;
using IronText.Algorithm;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class ConfigurableLrTable : ILrParserTable
    {
        private readonly CanonicalLrDfaTable actualLrTable;

        public ConfigurableLrTable(
            RuntimeOptions      flags,
            CanonicalLrDfaTable actualLrTable)
        {
            this.actualLrTable = actualLrTable;

            var relevantFlags = flags & RuntimeOptions.ParserAlgorithmMask;

            TargetRuntime
                = GetRuntime(relevantFlags, actualLrTable.TargetRuntime);

            ComplyWithConfiguration
                = relevantFlags != RuntimeOptions.ForceDeterministic
                || TargetRuntime == actualLrTable.TargetRuntime;
        }

        public bool ComplyWithConfiguration { get; }

        public ParserRuntime TargetRuntime { get; }

        public ITable<ParserAction> ParserActionTable => actualLrTable.ParserActionTable;

        public ParserConflictInfo[] Conflicts => actualLrTable.Conflicts;

        private static ParserRuntime GetRuntime(RuntimeOptions flags, ParserRuntime actualRuntime)
        {
            switch (flags)
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
    }
}
