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

        ForceGeneric          = 0x04,
    }
}
