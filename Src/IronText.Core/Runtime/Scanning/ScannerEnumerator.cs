using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IronText.Framework;
using IronText.Logging;

namespace IronText.Runtime
{
    sealed class ScannerEnumerator
        : IEnumerator<Msg>
    {
        private Scanner scanner;
        private int priorPosition;
        private int currentPosition;
        private int priorLine;
        private int priorColumn;

        private string                       document;
        private ScanCursor                   cursor;
        private readonly TextReader          textSource;
        private readonly TermFactoryDelegate termFactory;
        private readonly ILogging            logging;

        private readonly int[] actionToToken;

        internal ScannerEnumerator(Scanner scanner, ILogging logging)
        {
            this.scanner     = scanner;
            this.document    = scanner.document;
            this.textSource  = scanner.textSource;
            this.termFactory = scanner.termFactory;
            this.actionToToken = scanner.actionToToken;
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
                                Actions = new int[scanner.MaxActionCount],
                                ActionCount = 0,
                            };
            cursor.Buffer[0] = Scanner.Sentinel;

            this.priorPosition = 0;

            this.priorLine = 1;
            this.priorColumn = 1;
        }

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

                    if (cursor.ActionCount != 0)
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
                                Message = "Incomplete token",
                            });
                    }

                    return false;
                }

                if (cursor.ActionCount == 0) // No accepting state visited
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
            cursor.ActionCount = 0;
        }

        private int PrepareCurrent()
        {
            this.currentPosition += (cursor.Marker - cursor.Start);

            int    action = cursor.Actions[0];
            string text   = cursor.GetText();

            int token  = GetTokenFromAction(action);
            if (token >= 0)
            {
                int id = cursor.EnvelopeId;
                // TODO: Amb & Main tokens for envelope.Id
                Current = new Msg(id, token, action, text, new Loc(document, priorPosition, currentPosition), MakeHLoc());

                // Shrodinger's token
                if (cursor.ActionCount > 1)
                {
                    MsgData data = Current;
                    for (int i = 1; i != cursor.ActionCount; ++i)
                    {
                        action     = cursor.Actions[i];
                        token      = GetTokenFromAction(action);

                        data.Next = new MsgData(token, action, text);
                        data = data.Next;
                    }
                }
            }
            else
            {
                token = -1;
            }

            return token;
        }

        private int GetTokenFromAction(int action)
        {
            return actionToToken[action];
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
    }
}
