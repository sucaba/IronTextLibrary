namespace IronText.Reflection
{
    public enum ProductionFlags
    {
        None                = 0x0,

        HasSideEffects      = 0x1,

        /// <summary>
        /// 1 for automatically determining bottom-up or top-down
        /// 0 for using <c>BottomUpBehavior</c>
        /// </summary>
        AutoBehavior        = 0x2,

        /// <summary>
        /// 1 for bottom-up, 0 for top-down
        /// </summary>
        BottomUpBehavior    = 0x4
    }
}
