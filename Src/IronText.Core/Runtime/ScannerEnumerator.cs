using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IronText.Framework;
using IronText.Logging;

namespace IronText.Framework
{
    sealed class ScannerEnumerator
        : IEnumerator<Msg>
        , IScanning
    {
        private Scanner scanner;
        private int priorPosition;
        private int currentPosition;
        private int priorLine;
        private int priorColumn;

        private string document;
        private ScanCursor cursor;
        private readonly TextReader textSource;
        private readonly ScanActionDelegate termFactory;
        private readonly ILogging logging;
        private HLoc hLocation;
        private Loc location;
        private bool skipCurrentToken;

        internal ScannerEnumerator(Scanner scanner, ILogging logging)
        {
            this.scanner     = scanner;
            this.document    = scanner.document;
            this.textSource  = scanner.textSource;
            this.termFactory = scanner.scanAction;
            this.logging     = logging;

            this.cursor = new ScanCursor
                            {
                                RootContext = scanner.context,
                                CurrentMode = scanner.startMode,
                                Buffer   = new char[Scanner.BufferSize],
                                Cursor   = 0,
                                Limit    = 0,
                                Marker   = -1,
                                Start    = 0,
                                InnerState = 0,
                                ActionId = -1,
                            };
            cursor.Buffer[0] = Scanner.Sentinel;

            this.priorPosition = 0;

            this.priorLine = 1;
            this.priorColumn = 1;

            InitContext();
        }

        Loc IScanning.Location { get { return location; } }

        HLoc IScanning.HLocation { get { return hLocation; } }

        void IScanning.Skip() { this.skipCurrentToken = true; }

        public Msg Current { get; private set; }

        public void Dispose()
        {
            this.cursor = null;
        }

        object System.Collections.IEnumerator.Current { get { return Current; } }

        public bool MoveNext()
        {
            while (true)
            {
                int token;

                int required = cursor.CurrentMode(cursor);
                if (required != 0)
                {
                    Debug.Assert(
                            cursor.Cursor == cursor.Limit && cursor.Buffer[cursor.Cursor] == Scanner.Sentinel,
                            "Buffering should be requested only on sentinel when limit==cursor");

                    if (!cursor.IsEoi)
                    {
                        Fill();
                        continue;
                    }

                    if (cursor.ActionId >= 0)
                    {
                        token = PrepareCurrent();
                        CompleteToken();
                        if (token < 0)
                        {
                            continue;
                        }

                        return true;
                    }

                    if (cursor.Start != cursor.Cursor)
                    {
                        logging.Write(
                            new LogEntry
                            {
                                Severity = Severity.Error,
                                Location = new Loc(
                                                document,
                                                cursor.Cursor,
                                                cursor.Cursor),
                                HLocation = new HLoc(
                                                cursor.CursorLine,
                                                cursor.CursorColumn - 1,
                                                cursor.CursorLine,
                                                cursor.CursorColumn - 1),
                                Message = "Unexpected end of input",
                            });
                    }

                    return false;
                }

                if (cursor.ActionId < 0) // No accepting state visited
                {
                    char badCh = cursor.Buffer[cursor.Cursor];
                    var message = string.Format(
                            "Unexpected character '{0}' (\\U{1:X4}).",
                            char.IsControl(badCh) ? '?' : badCh,
                            (int)badCh);

                    // TODO: Handle newlines in unexpected input

                    this.currentPosition += (cursor.Cursor - cursor.Start);
                    logging.Write(
                        new LogEntry
                        {
                            Severity  = Severity.Error,
                            Location  = new Loc(document, currentPosition, currentPosition + 1),
                            HLocation = new HLoc(
                                            cursor.CursorLine,
                                            cursor.CursorColumn,
                                            cursor.CursorLine,
                                            cursor.CursorColumn),
                            Message   = message
                        });

                    cursor.Marker = cursor.Cursor + 1;
                    token = -1;
                }
                else
                {
                    // TODO: handle tokens longer than buffer
                    token = PrepareCurrent();
                }

                CompleteToken();

                if (token >= 0)
                {
                    return true;
                }
            }
        }

        private void CompleteToken()
        {
            priorPosition = currentPosition;
            if (cursor.Buffer[cursor.Marker - 1] == '\n')
            {
                priorLine = cursor.MarkerLine + 1;
                priorColumn = 1;
            }
            else
            {
                priorLine = cursor.MarkerLine;
                priorColumn = cursor.MarkerColumn;
            }

            cursor.Start = cursor.Cursor = cursor.Marker;
            cursor.InnerState = 0;
            cursor.ActionId = -1;
        }

        private int PrepareCurrent()
        {
            int token;
            this.currentPosition += (cursor.Marker - cursor.Start);

            this.hLocation = MakeHLoc();
            this.location = new Loc(document, priorPosition, currentPosition);

            object tokenValue;
            this.skipCurrentToken = false;
            token = termFactory(cursor, out tokenValue);

            if (token >= 0 && !skipCurrentToken)
            {
                Current = new Msg(token, tokenValue, location, hLocation);
            }
            else
            {
                token = -1;
            }

            return token;
        }

        private HLoc MakeHLoc()
        {
            var result = new HLoc(
                                priorLine,
                                priorColumn,
                                cursor.MarkerLine,
                                cursor.MarkerColumn - 1);
            return result;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        private void Fill()
        {
            // Shift data to the buffer start
            int len = cursor.Limit - cursor.Start;
            if (cursor.Start != 0)
            {
                Array.ConstrainedCopy(cursor.Buffer, cursor.Start, cursor.Buffer, 0, len);

                cursor.Marker -= cursor.Start;
                cursor.Cursor -= cursor.Start;
                cursor.Limit  -= cursor.Start;

                // Following can be negative but it is ok after adding buffer start position
                cursor.MarkerLineStart -= cursor.Start; 
                cursor.CursorLineStart -= cursor.Start;

                cursor.Start = 0;
            }

            int toRead = cursor.Buffer.Length - len - 1; // - 1 for sentinel
            int count = textSource.Read(cursor.Buffer, len, toRead);
            cursor.IsEoi = count < toRead;
            len += count;
            cursor.Buffer[len] = Scanner.Sentinel;

            cursor.Limit = len;
        }

        private void InitContext()
        {
            var rootContext = cursor.RootContext;
            if (rootContext != null)
            {
                ServicesInitializer.SetServiceProperties(
                    rootContext.GetType(),
                    rootContext,
                    typeof(IScanning),
                    this);
            }
        }
    }
}
