namespace IronText.Runtime
{
    public enum ParserOperation : byte
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

        // Success
        Accept      = 5,

        // Return control to the parser
        Exit        = 6,

        // Continue parser loop
        Restart     = 7,

        InternalError = 8
    }
}
