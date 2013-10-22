using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework
{
    public enum LanguageFlags
    {
        Default               = 0x0,

        /// <summary>
        /// Any conflicts in parsing tables will cause build-time errors.
        /// </summary>
        ForceDeterministic    = 0x00,

        /// <summary>
        /// Non-deterministic parser (GLR) will be used automatically if parser table contains conflicts.
        /// </summary>
        AllowNonDeterministic = 0x01,

        /// <summary>
        /// Non-deterministic parser (GLR) will be used even if parser table contains no conflicts.
        /// </summary>
        /// <remarks>
        /// Note: In most cases non-deterministic parser algorithm is significantly slower than its
        /// deterministic counterpart.
        /// </remarks>
        ForceNonDeterministic = 0x02,

        ParserAlgorithmMask   = 0x03,

        /// <summary>
        /// Resolve lexical ambiguities by using rule which is defined first
        /// </summary>
        FirstScanRuleWins     = 0x4,

        /// <summary>
        /// Resolve lexical ambiguities by using all scan rules 
        /// and packing produced tokens into a Shrodinger's token.
        /// </summary>
        /// <remarks>
        /// Sometimes such ambiguous tokens can cause shift-shift
        /// conflicts in parser, hence such grammar will require
        /// GLR algorithm.
        /// </remarks>
        UseShrodingerToken    = 0x8,

        ScanAmbiguitiesResolutionMask = 0xc
    }
}
