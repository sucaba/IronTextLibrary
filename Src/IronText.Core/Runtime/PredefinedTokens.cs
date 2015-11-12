namespace IronText.Runtime
{
    public static class PredefinedTokens
    {
        internal const int NoToken      = -1;

        internal const int Epsilon      = 0;

        internal const int Propagated   = 1;

        internal const int AugmentedStart = 2;

        public const int Eoi            = 3;

        public const int Error          = 4;

        public const int Count          = 5;

        public static readonly int[] All = new [] { Epsilon, Propagated, AugmentedStart, Eoi, Error };
    }
}
