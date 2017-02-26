using IronText.Automata.Lalr1;
using IronText.Reporting;
using IronText.Runtime;
using System;

namespace IronText.MetadataCompiler
{
    class ParserRuntimeDesignator
    {
        public ParserRuntimeDesignator(
            RuntimeOptions       options,
            CanonicalLrDfaTable  table,
            ParserConflictInfo[] parserConflicts)
        {
            var relevantOptions = options & RuntimeOptions.ParserAlgorithmMask;

            var minimalRuntime = 
                (parserConflicts.Length != 0 || table.HasUnresolvedTerminalAmbiguities)
                ? ParserRuntime.Glr
                : ParserRuntime.Deterministic;

            ActualRuntime = GetRuntime(options, minimalRuntime);

            ComplyWithConfiguration
                = relevantOptions != RuntimeOptions.ForceDeterministic
                || ActualRuntime == minimalRuntime;
        }

        public ParserRuntime ActualRuntime          { get; }

        public bool          ComplyWithConfiguration { get; }

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
