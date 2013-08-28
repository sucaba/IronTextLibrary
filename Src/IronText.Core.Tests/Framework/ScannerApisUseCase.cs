using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class ScannerApisUseCase
    {
        const int SpaceTokenId = 0;
        const int NumberTokenId = 1;

        [Test]
        public void Test()
        {
            const string document = "number.list";

            string[] expectedTokens = new string[] { " ", "123", "   ", "0"};
            int[] expectedTokenIds = new int[] { SpaceTokenId, NumberTokenId, SpaceTokenId, NumberTokenId};
            Loc[] expectedLocations = new Loc[] {
                new Loc(document, 0, 1),
                new Loc(document, 1, 4),
                new Loc(document, 4, 7),
                new Loc(document, 7, 8) 
            };

            var input = new StringReader(string.Join("", expectedTokens));

            ScanActionDelegate scanAction = (ScanCursor cursor, out object value)
                =>
                {
                    value = new string(cursor.Buffer, cursor.Start, cursor.Marker - cursor.Start);
                    return cursor.ActionId;
                };

            var logging = ExceptionLogging.Instance;
            var allTokens = new Scanner(Scan1, input, document, null, scanAction, logging).ToArray();

            Assert.AreEqual(expectedTokens, TokenTexts(allTokens));
            Assert.AreEqual(expectedTokenIds, TokenIds(allTokens));
            Assert.AreEqual(expectedLocations, Locations(allTokens));

            var scanner = new Scanner(Scan1, new StringReader(" a11"), Loc.MemoryString, null, scanAction, logging);
            Assert.Throws<SyntaxException>(
                () =>
                {
                    scanner.ToArray();
                });
        }

        private static string[] TokenTexts(IEnumerable<Msg> msgs)
        {
            return msgs.Select(m => (string)m.Value).ToArray();
        }

        private static int[] TokenIds(IEnumerable<Msg> msgs)
        {
            return msgs.Select(m => m.Id).ToArray();
        }

        private static Loc[] Locations(IEnumerable<Msg> msgs)
        {
            return msgs.Select(m => m.Location).ToArray();
        }

        private int Scan1(ScanCursor cursor)
        {
            char ch;

            switch (cursor.InnerState)
            {
                case 0: goto M0;
                case 1: goto MDIGIT;
                case 2: goto MSPACE;
                default: goto L0;
            }
        FIN:
            return 0;

        L0:
            // State #0

        M0:
            ch = cursor.Buffer[cursor.Cursor];

            if (ch == Scanner.Sentinel)
            {
                if (cursor.Limit == cursor.Cursor)
                {
                    cursor.InnerState = 0;
                    return 1;
                }

                // Non accept
                goto FIN;
            }
            else if (ch == ' ')
            {
                goto LSPACE;
            }
            else if (char.IsDigit(ch))
            {
                goto LDIGIT;
            }
            else
            {
                goto FIN;
            }

        LDIGIT: // State #1
            ++cursor.Cursor;
            cursor.ActionId = 1;
            cursor.Marker = cursor.Cursor; // accept
        MDIGIT:
            ch = cursor.Buffer[cursor.Cursor];
            if (ch == Scanner.Sentinel)
            { 
                if (cursor.Limit == cursor.Cursor)
                {
                    cursor.InnerState = 1;
                    return 1;
                }

                // Non accept
                goto FIN;
            }
            else if (char.IsDigit(ch))
            {
                goto LDIGIT;
            }
            else
            {
                goto FIN;
            }

        LSPACE: // State #2
            ++cursor.Cursor;
            cursor.ActionId = 0;
            cursor.Marker = cursor.Cursor;
        MSPACE:
            ch = cursor.Buffer[cursor.Cursor];
            if (ch == Scanner.Sentinel)
            {
                if (cursor.Limit == cursor.Cursor)
                {
                    cursor.InnerState = 2;
                    return 1;
                }

                goto FIN;
            }
            else if (ch == ' ')
            {
                goto LSPACE;
            }
            else
            {
                goto FIN;
            }
        }
    }
}
