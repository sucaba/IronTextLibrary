using System.Collections.Generic;
using System.IO;

namespace IronText.Framework
{
    sealed class Scanner
        : IScanner
        , IEnumerable<Msg>
    {
        public const char Sentinel = '\0';
        public const int BufferSize = 1024;

        internal readonly object             context;
        internal readonly string             document;
        internal readonly TextReader         textSource;
        internal readonly Scan1Delegate      startMode;
        internal readonly ScanActionDelegate scanAction;
        private readonly ILogging logging;

        public Scanner(
            Scan1Delegate      startMode,
            TextReader         textSource,
            string             document,
            object             rootContext,
            ScanActionDelegate scanAction,
            int                maxActionCount,
            ILogging           logging)
        {
            this.startMode  = startMode;
            this.textSource = textSource;
            this.context    = rootContext;
            this.document   = document;
            this.scanAction = scanAction;
            this.MaxActionCount = maxActionCount;
            this.logging    = logging;
        }

        public int MaxActionCount { get; private set; }

        public IEnumerator<Msg> GetEnumerator()
        {
            return new ScannerEnumerator(this, logging);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public IReceiver<Msg> Accept(IReceiver<Msg> visitor)
        {
            var enumerator = new ScannerEnumerator(this, logging);
            while (enumerator.MoveNext())
            {
                if (visitor == null)
                {
                    return null;
                }

                visitor = visitor.Next(enumerator.Current);
            }

            if (visitor != null)
            {
                return visitor.Done();
            }

            return null;
        }
    }
}
