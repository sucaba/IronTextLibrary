namespace IronText.Framework
{
    public class ScanCursor
    {
        public object RootContext;
        public Scan1Delegate CurrentMode;

        public char[] Buffer;  // buffer 
        public int Limit;      // end of data in buffer
        public bool IsEoi;     // EOI flag
        public int Start;      // token start
        public int InnerState; // Inner state of the scanner FSM
        public int CurrentActionId;   // accept action ID

        public int   EnvelopeId;
        public int   ActionCount;
        public int[] Actions;

        public int Cursor;          // current scan position
        public int CursorLine = 1;  // 1-based currently scanned line number
        public int CursorLineStart; // buffer line-start position of the currently scanned line
        public int CursorColumn
        {
            get { return Cursor - CursorLineStart + 1; }
        }

        public int Marker;          // accepted token end
        public int MarkerLine = 1;  // 1-based last line of the matched token
        public int MarkerLineStart; // buffer line-start position of last matched token's line 
        public int MarkerColumn
        {
            get { return Marker - MarkerLineStart + 1; }
        }
    }
}
