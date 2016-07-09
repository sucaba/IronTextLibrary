using IronText.Logging;
using System.Collections.Generic;
using System.IO;

namespace IronText.Runtime
{
    sealed class Scanner
        : IScanner
        , IEnumerable<Message>
    {
        public const char Sentinel = '\0';
        public const int BufferSize = 1024;

        internal readonly object             context;
        internal readonly string             document;
        internal readonly TextReader         textSource;
        internal readonly Scan1Delegate      startMode;
        internal readonly int[]              actionToToken;

        private  readonly ILogging           logging;

        public Scanner(
            Scan1Delegate startMode,
            TextReader textSource,
            string document,
            object rootContext,
            int maxActionCount,
            int[] actionToToken,
            ILogging logging)
        {
            this.startMode  = startMode;
            this.textSource = textSource;
            this.context    = rootContext;
            this.document   = document;
            this.MaxActionCount = maxActionCount;
            this.actionToToken = actionToToken;
            this.logging    = logging;
        }

        public int MaxActionCount { get; private set; }

        public IEnumerator<Message> GetEnumerator()
        {
            return new ScannerEnumerator(this, logging);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public IReceiver<Message> Accept(IReceiver<Message> visitor)
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
