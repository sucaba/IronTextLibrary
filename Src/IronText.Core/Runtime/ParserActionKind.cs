namespace IronText.Runtime
{
    public enum ParserActionKind : ushort
    {
        // Unexpected token
        Fail        = 0,

        // Accept token and shift to the next state
        Shift       = 1,

        // Reduce rule and invoke rule action
        Reduce      = 2,

        // Resolve Shrodinger's token
        Resolve     = 3,

        // Fork on allowed alternatives of a Shrodinger's token
        Fork        = 4,

        // Multiple actions can happen in this state (0 or 1 shift + 0 or more reductions)
        Conflict    = 5,

        // Success
        Accept      = 6,

        // Return control to the parser
        Exit        = 7,

        // Continue parser loop
        Restart    = 8,

        InternalError = 9
    }
}
