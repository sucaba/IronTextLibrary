using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Bootstrapping lexer based on the Regex class.
    /// </summary>
    class BootstrapScanner : IScanner, IEnumerable<Msg>
    {
        private readonly Regex regex;
        private readonly string text;
        private ScannerDescriptor descriptor;
        private readonly object rootContext;
        private readonly ILogging logging;

        public BootstrapScanner(
                string input,
                ScannerDescriptor descriptor,
                object rootContext,
                ILogging logging)
            : this(new StringReader(input), Loc.MemoryString, descriptor, rootContext, logging)
        { }

        public BootstrapScanner(
                TextReader        textSource,
                string            document,
                ScannerDescriptor descriptor,
                object            rootContext,
                ILogging          logging)
        {
            this.descriptor = descriptor;
            this.rootContext = rootContext;
            this.logging = logging;

            var pattern = @"\G(?:" + string.Join("|", descriptor.Matchers.Select(scanProd => "(" + GetPattern(scanProd) + ")")) + ")";
            this.regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            this.text = textSource.ReadToEnd();
        }

        private static string GetPattern(Matcher production)
        {
            return production.Pattern.BootstrapPattern;
        }

        public IReceiver<Msg> Accept(IReceiver<Msg> visitor)
        {
            return visitor.Feed(Tokenize()).Done();
        }

        private IEnumerable<Msg> Tokenize()
        {
            int currentPos = 0;

            if (text.Length != 0)
            {
                var match = this.regex.Match(this.text);
                for (; match.Success; match = match.NextMatch())
                {
                    currentPos = match.Index + match.Length;

                    int action = Enumerable
                                    .Range(1, match.Groups.Count)
                                    .First(i => match.Groups[i].Success) - 1;
                    var matcher = descriptor.Matchers[action];
                    if (matcher.Outcome == null)
                    {
                        continue;
                    }

                    var detOutcome = (Symbol)matcher.Outcome; // BootstrapScanner does not support lexical ambiguities
                    int token = detOutcome.Index;

                    yield return
                        new Msg(token, match.Value, action, new Loc(Loc.MemoryString, match.Index, match.Length));

                    if (currentPos == text.Length)
                    {
                        break;
                    }
                }

                if (!match.Success)
                {
                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Message = "Unexpected char: '" + text[currentPos] + "' at " + (currentPos + 1)
                        });
                }
            }
        }

        public IEnumerator<Msg> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
