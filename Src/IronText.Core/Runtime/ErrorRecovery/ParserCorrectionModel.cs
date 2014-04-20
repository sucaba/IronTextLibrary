using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Logging;
using IronText.Reflection;

namespace IronText.Runtime
{
    class ParserCorrectionModel : List<int>
    {
        public const int Spelling = -1;
        public const int Insertion = -2;

        private readonly string messageFormat;
        private int requiredInputSize;
        private int minimalInputSize;

        public ParserCorrectionModel(string messageFormat, int requiredInputSize = 0)
        {
            this.messageFormat = messageFormat;
            this.requiredInputSize = requiredInputSize;
        }

        public Loc GetHiglightLocation(List<Msg> source)
        {
            for (int i = 0; i != Count; ++i)
            {
                if (this[i] != i)
                {
                    if (source[i].Location.IsUnknown)
                    {
                        break;
                    }

                    return source[i].Location;
                }
            }

            var errorItems = source.Select(s => s.Location);
            if (source.Count > 1)
            {
                errorItems = errorItems.Skip(1);
            }

            return Loc.Sum(errorItems);
        }

        public HLoc GetHiglightHLocation(List<Msg> source)
        {
            for (int i = 0; i != Count; ++i)
            {
                if (this[i] != i)
                {
                    if (source[i].HLocation.IsUnknown)
                    {
                        break;
                    }

                    return source[i].HLocation;
                }
            }

            var errorItems = source.Select(s => s.HLocation);
            if (source.Count > 1)
            {
                errorItems = errorItems.Skip(1);
            }

            return HLoc.Sum(errorItems);
        }

        public string FormatMessage(RuntimeGrammar grammar, List<Msg> source, List<Msg> corrected)
        {
            var output = new StringBuilder();
            ProcessMessageFormat(grammar, source, corrected, output);
            return output.ToString();
        }

        private void ProcessMessageFormat(
            RuntimeGrammar grammar,
            List<Msg>          source,
            List<Msg>          corrected,
            StringBuilder      output)
        {
            int i = 0;
            int count = messageFormat.Length;
            while (i != count)
            {
                char ch = messageFormat[i];
                switch (ch)
                {
                    case '%':
                        ++i;
                        if (i == count || !char.IsDigit(messageFormat[i]))
                        {
                            throw new InvalidOperationException("Invalid message format.");
                        }

                        int correctedIndex = messageFormat[i++] - '0';
                        output.Append(FormatToken(grammar, corrected[correctedIndex]));
                        break;
                    case '$':
                        ++i;
                        if (i == count || !char.IsDigit(messageFormat[i]))
                        {
                            throw new InvalidOperationException("Invalid message format.");
                        }

                        int sourceIndex = messageFormat[i++] - '0';
                        output.Append(FormatToken(grammar, source[sourceIndex]));
                        break;
                    default:
                        output.Append(messageFormat[i++]);
                        break;
                }
            }
        }

        private string FormatToken(RuntimeGrammar grammar, Msg msg)
        {
            if (msg.AmbToken == PredefinedTokens.Eoi)
            {
                return "end of file";
            }

            string result;

            if (!string.IsNullOrEmpty(msg.Text))
            {
                result = "'" + msg.Text + "'";
            }
            else
            {
                // TODO: Get rid of grammar usage. Properly formatted text should be sufficient.
                result = grammar.SymbolName(msg.AmbToken);
            }

            return result;
        }

        public int GetRequiredInputSize()
        {
            if (requiredInputSize == 0)
            {
                requiredInputSize = this.Max() + 1;
            }

            return requiredInputSize;
        }

        // Minimal matching length
        public int GetMinimalLength()
        {
            if (minimalInputSize == 0)
            {
                int size = GetRequiredInputSize();
                var original = Enumerable.Range(0, size).Reverse().ToArray();
                var reversed = Enumerable.Reverse(this).ToArray();

                minimalInputSize = size;  
                
                int maxCommonTailCount = Math.Min(size, Count);

                for (int i = 0; i != maxCommonTailCount; ++i)
                {
                    if (original[i] != reversed[i])
                    {
                        break;
                    }

                    --minimalInputSize;
                }
            }

            return minimalInputSize;
        }

        public IEnumerable<int> GetDeletedIndexes()
        {
            int size = GetRequiredInputSize();
            return Enumerable
                    .Range(0, size)
                    .Except(this)
                    .ToArray();
        }
    }
}
