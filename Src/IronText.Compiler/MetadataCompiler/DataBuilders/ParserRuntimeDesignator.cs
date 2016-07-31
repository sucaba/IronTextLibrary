using IronText.Automata.Lalr1;
using IronText.Runtime;
using System;

namespace IronText.MetadataCompiler
{
    class ParserRuntimeDesignator
    {
        public ParserRuntimeDesignator(RuntimeOptions options, CanonicalLrDfaTable table)
        {
            var relevantOptions = options & RuntimeOptions.ParserAlgorithmMask;

            ActualRuntime = GetRuntime(options, table.TargetRuntime);

            ComplyWithConfiguration
                = relevantOptions != RuntimeOptions.ForceDeterministic
                || ActualRuntime == table.TargetRuntime;
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
